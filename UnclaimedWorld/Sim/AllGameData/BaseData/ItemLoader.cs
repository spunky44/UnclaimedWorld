using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.ClientSide;
using UWGame.SimSide.Combat;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.Client.Particles;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.SimSide.AllGameData
{

    public class ItemLoader
    {
        public const float toolDurabilityBrittle = 0f;
        public const float toolDurabilityAverage = 0.5f;
        public const float toolDurabilityDurable = 0.9f;
        public const float toolDurabilityUnbreakable = 1f;

        public const float assemblerPlateDurability = 0.8f;

        public static void Init(List<EntityType> listOfEntityTypes)
        {

            // descriptions. TODO: divide in 2 parts, the upper part from the database (neutral language), the lower part 'NOTES'  by camp members... ideas, improvised uses, ideas for recipes etc



            listOfEntityTypes.Add(new EntityType("item:meshTest")
            {
                Name = "Mesh test",
                SummaryDescription = "test the mesh", 
                ItemType = new ItemType() { MaximumBulk = 0.15f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType()
                {
                    RenderAsModelType = new RenderAsModelType() { AssetName = "meshtest", ModelScale = 10f }
                },
            });

                #region FOOD

            #region astroRation
            listOfEntityTypes.Add(new EntityType("item:astroRation") // contains recommended daily nutrition for a grown man, weighs the amount suggested in the drive doc. see also foodnutrientprofiles in gamedataloader and needs under creatureloader
            {
                Name = "PRECOL Ration",
                SummaryDescription = "Meal. Consists of a variety of synthetic food items",
                Description = "Can be stored under any conditions and will keep almost indefinitely.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,   
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowWeightBalancedMeal"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    DegradeType = "dirt",
                    Repairability = 0f,

                    DegradesTo = "item:spoiledMeal"
                },
                TierOrArea = new TierOrArea() { Tier = "advanced" }, // had Area = RatingTypes.Food
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } }} 
            });
            #endregion
            #region blackpulp
            listOfEntityTypes.Add(new EntityType("item:blackpulp")
            {
                Name = "Blackpulp",
                SummaryDescription = "Inside the tough, rubbery shell of these fruits is a tar-like substance",
                Description = "Contains toxins that make it inedible to humans without some advanced processing",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,


                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"], FoodTags = new string[] { "inedibleVegi" } }
                },
                NonLivingType = new NonLivingType()
                {
                    DegradeType = "perishable",
                    Repairability = 0f,

                    DegradesTo = "item:rottenVegetables"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "blackpulp" } } }}
            });
            #endregion
            #region blackzpacho
            listOfEntityTypes.Add(new EntityType("item:blackzpacho")
            {
                Name = "Blackzpacho",
                SummaryDescription = "Cold soup made from blackpulp",
                Description = "The Tau Ceti variation of the Earth's gazpacho. To make this, the blackpulp must be processed with a specially engineered enzyme (that we can produce in a field lab).",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,


                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region minnowSoup
            listOfEntityTypes.Add(new EntityType("item:minnowSoup")
            {
                Name = "Minnow soup",
                SummaryDescription = "Soup made from small fish",
                Description = "Lacking a larger catch, minnows can be used in a soup. The result varies, depending on the vegetables and spices used.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["meatSoup"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } },

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBeige" } } }}
            });
            #endregion
            #region favorbread
            listOfEntityTypes.Add(new EntityType("item:favorbread")
            {
                Name = "Favorbread",
                SummaryDescription = "A fleshy, nutritious vegetable which is edible to humans without preparation",
                Description = "A nourishing staple which can be found at the foot of the sanctuary tree. On dead sanctuary trees, the favorbread naturally ceases to grow but in these locations we have found a way to revive it with a simple method of cultivation: By excavating a small garden directly underneath the dead sanctuary tree and providing the carbohydrates that the tree no longer supplies it with.",//
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,


                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"], FoodTags = new string[] { "edibleVegi" } }
                },
                NonLivingType = new NonLivingType()
                {
                    DegradeType = "perishable",
                    Repairability = 0f,

                    DegradesTo = "item:rottenVegetables"
                },
                CategoryKey = "preparedFood", 
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "favorbread" } } } }
            });
            #endregion
            #region fingerFruit
            listOfEntityTypes.Add(new EntityType("item:fingerFruit")
            {
                Name = "Finger fruit",
                SummaryDescription = "A nutritious vegetable resembling a small sausage. Requires cooking",
                Description = "We can cultivate the finger fruit plant in a greenhouse.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"] } //dont let it be edible raw, because it's a seed for greenhouse , cannot have agents eat it.
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "perishable, no freeze",
                    DegradesTo = "item:rottenVegetables",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "vegetables" } } }}
            });
            #endregion
            #region finger pot
            listOfEntityTypes.Add(new EntityType("item:fingerPot") //fingerFruitPot...
            {
                Name = "Finger pot", // 
                SummaryDescription = "Vegetable dish made from fresh finger fruit",
                Description = "N/A",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,


                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } } }
            });
            #endregion
            #region fermented finger fruit
            listOfEntityTypes.Add(new EntityType("item:fermentedFingerFruit") //
            {
                Name = "Fermented finger fruit", // 
                SummaryDescription = "Finger fruit which has fermented in brine, increasing its lifespan", //mp: salt and sweet?
                Description = "Will keep for a long time. The taste is a unique combination of salty, sweet and sour with distinctive flowery aromas.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,
                    RequiredStorageTags = new[] { "storageTagLiquidContainerClosedNoHeat" },
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "pickledFood", //
                    DegradesTo = "item:spoiledMeal"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } } }
            });
            #endregion

 //mp note that vinegar is placed under tools, below

            #region driedBeef
            listOfEntityTypes.Add(new EntityType("item:driedBeef")
            {
                Name = "Beef jerky",
                SummaryDescription = "Beef jerky stays fresh for several months",
                Description = "Meat, trimmed of fat, salted and dried to prevent spoilage.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } }}
            });
            #endregion
            #region waterCaneSeeds
            listOfEntityTypes.Add(new EntityType("item:waterCaneSeeds")
            {
                Name = "Water cane seeds",
                SummaryDescription = "Found on the dead water cane",
                Description = "The seeds have some nutritional value and can even be eaten raw although thorough cooking is recommended.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                        
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"], FoodTags = new string[] { "edibleVegi" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:rottenStaple",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "staple" } } }}
            });
            #endregion
            #region waterCanePorridge
            listOfEntityTypes.Add(new EntityType("item:waterCanePorridge")
            {
                Name = "Water cane porridge",
                SummaryDescription = "A modest bush dish which provides much needed calories",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f, // Lars: since it is a staple I increased the portion size, also requires 2 seed items to make


                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region hexapineLeaves
            listOfEntityTypes.Add(new EntityType("item:hexapineLeaves")
            {
                Name = "Hexapine leaves",
                SummaryDescription = "The leaves from the hexapine need enzyme treatment to become edible.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.125f,


                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"], FoodTags = new string[] { "inedibleVegi" } }  // MP: question: should this have  IsMeal = true,  why/why not. Answer (10/9 2014): the IsMeal = true is used to determine if humans would rather wait with eating a prepared meal instead of eating the edible ingredients instantly.
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "perishable, no freeze",
                    DegradesTo = "item:rottenVegetables"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "vegetables" } } } }
            });
            #endregion
            #region hexapineSalad
            listOfEntityTypes.Add(new EntityType("item:hexapineSalad")
            {
                Name = "Hexapine slaw", // http://en.wikipedia.org/wiki/Coleslaw
                SummaryDescription = "Vegetable dish made from enzyme-treated hexapine leaves.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,


                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } }}
            });
            #endregion
            #region turnipMeat
            listOfEntityTypes.Add(new EntityType("item:turnipMeat")
            {
                Name = "Turnip meat",
                SummaryDescription = "The meat is hard to extract from the turnip's shell.",
                Description = "Has a characteristic blue tinge and a flavor that takes some getting used to.",
                ItemType = new ItemType()
                {

                    MaximumBulk = 0.07f,
                        FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw meat",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } }}
            });
            #endregion

            #region turnipbrain 
            listOfEntityTypes.Add(new EntityType("item:turnipBrain")
            {
                Name = "Turnip brain", //http://animals.howstuffworks.com/mammals/deer-tan-own-hide1.htm
                SummaryDescription = "A rather small brain for such a large animal",//
                Description = "The brain contains oils that can be used as a primitive tanning agent for hide treatment.",//
                ToolType = new ToolType() { Durability = 0.2f, ToolHandling = ToolHandlingType.HandTool }, //tool for tanning hides
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.15f,
                        FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw meat",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gutsyellow" } } }}
            });
            #endregion
            #region turnipGuts
            listOfEntityTypes.Add(new EntityType("item:turnipGuts")
            {
                Name = "Turnip guts",
                SummaryDescription = "The vast guts of a large grass eater",//
                Description = "These intestines can find use in cooking and possibly as a natural material.",//
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                        FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw meat",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gutspink" } } }}
            });
            #endregion
            #region turnipTentacles
            listOfEntityTypes.Add(new EntityType("item:turnipTentacles")
            {
                Name = "Turnip tentacles",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.15f,
                        FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tentacles" } } }}
            });
            #endregion
            #region turnipRoastMeal //not used. delete
            /*           listOfEntityTypes.Add(new EntityType("item:turnipRoastMeal")  // a single serving of Turnip Roast //don't use
            {
                Name = "Turnip roast meal",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal"
                },
                Category = GameData.Instance.AllItemCategories["preparedFood"],
                RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } } }
            });*/
            #endregion
            #region turnipRoast
            listOfEntityTypes.Add(new EntityType("item:turnipRoast")
            {
                Name = "Turnip roast",
                SummaryDescription = "Big, soft pieces of roasted meat with a characteristic blue tinge",
                Description = "N/A", //placeholder txt
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } } }
            });
            #endregion
            #region turnipStew //not used
     /*       listOfEntityTypes.Add(new EntityType("item:turnipStew")
            {
                Name = "Turnip stew",
                SummaryDescription = "A tasty, gelatinous stew with the peculiar blue tinge that characterizes dishes made from the turnip animal", //
                Description = "", //placeholder txt
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.125f,
                    FoodType = new FoodType()
                    {
                        FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorMeat"],
                        IsMeal = true,
                        FoodTags = new string[] { "cookedMeat" }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                Category = GameData.Instance.AllItemCategories["preparedFood"],
                RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } } }
            });*/
            #endregion
            #region smokedTurnip
            listOfEntityTypes.Add(new EntityType("item:smokedTurnip")
            {
                Name = "Smoked turnip meat",
                SummaryDescription = "Pieces of smoked meat from the turnip animal. Will keep for some time", //
                Description = "N/A", //placeholder txt
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "somewhatPreserved",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } } }
            });
            #endregion
            #region turnipRawSausage
            listOfEntityTypes.Add(new EntityType("item:turnipRawSausage")
            {
                Name = "Turnip raw sausage",
                SummaryDescription = "Fermented sausages made from turnip meat. Can be cooked or made into salami",
                Description = "Yeast-like bacteria have fermented these sausages. They can now either be dried, making long-lasting salami or they can be cooked for immediate consumption. The sausages have a limited shelf life in the current condition.",
                ItemType = new ItemType()
                {

                    MaximumBulk = 0.05f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },              
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw meat",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } } }
            });
            #endregion
            #region turnipSalami
            listOfEntityTypes.Add(new EntityType("item:turnipSalami")
            {
                Name = "Turnip salami",
                SummaryDescription = "A tasty, ready to eat salami which can keep for months",
                Description = "This meat product is the result of careful craftsmanship. It is made from fermented sausages that are hung to dry for a long time in very specific humidity and temperature conditions.", //
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } } }
            });
            #endregion
            #region turnipFriedSausage
            listOfEntityTypes.Add(new EntityType("item:turnipFriedSausage")
            {
                Name = "Turnip fried sausage",
                SummaryDescription = "Juicy sausages made from turnip meat. They have a characteristic blue tinge",
                Description = "N/A", //placeholder txt
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } } }
            });
            #endregion
            #region twinklerGuts
            listOfEntityTypes.Add(new EntityType("item:twinklerGuts")  // not used..
            {
                Name = "Quadite innards",
                SummaryDescription = "N/A", // not used..
                Description = "N/A", //
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.15f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "waste",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gutspink" } } }}
            });
            #endregion
            #region twinklerMeat
            listOfEntityTypes.Add(new EntityType("item:twinklerMeat")
            {
                Name = "Quadite flesh",
                SummaryDescription = "Riddled with scamp larvae (which in time become edible)",
                Description = "\n BIOLOGY OVERVIEW\n The Scamp larvae start their life as parasites inside the quadite's digestive canals. After the animal dies of unrelated causes, the larvae will quickly devour its flesh, growing in size and ending the feeding frenzy when the biggest grub devours its siblings.\n \nSURVIVAL GUIDE NOTES\n The scamp larvae secrete harmful substances designed to ward off other carrion-eaters, but once their feeding ends, the single scamp that remains can be cooked and eaten by humans. This requires that the meat be stored for a while, closely monitored so that the grub can be caught before it transitions into its next stage, the Scamp beetle. (The beetle is not to be eaten, even after cooking.)",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.08f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:scampGrub"
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gutspink" } } }}
            });
            #endregion
            #region shreds
            #region twinkelrShred
            listOfEntityTypes.Add(new EntityType("item:twinklerShred")
            {
                Name = "Quadite shreds",
                SummaryDescription = "Shreds of quadite flesh, torn off the carcass by scavengers",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    //  don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } }}
            });
            #endregion
            #region binalRatShred
            listOfEntityTypes.Add(new EntityType("item:binalRatShred")
            {
                Name = "Binal rat shreds",
                SummaryDescription = "Small remnants of a binal rat, torn off by animals that feed on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } }}
            });
            #endregion
            #region thunderChickenShred
            listOfEntityTypes.Add(new EntityType("item:thunderChickenShred")
            {
                Name = "Thunder chicken shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } }}
            });
            #endregion
            #region patricianShred
            listOfEntityTypes.Add(new EntityType("item:patricianShred")
            {
                Name = "Patrician shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region BushDragonShred
            listOfEntityTypes.Add(new EntityType("item:bushDragonShred")
            {
                Name = "Bush dragon shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region TurnipShred
            listOfEntityTypes.Add(new EntityType("item:turnipShred")
            {
                Name = "Turnip shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region demonTreeShred
            listOfEntityTypes.Add(new EntityType("item:demonTreeShred")
            {
                Name = "Dendront shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region swampDemonTreeShred
            listOfEntityTypes.Add(new EntityType("item:swampDemonTreeShred")
            {
                Name = "Swamp tree demon shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region megapodShred
            listOfEntityTypes.Add(new EntityType("item:megapodShred")
            {
                Name = "Megapod shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region whipjawShred
            listOfEntityTypes.Add(new EntityType("item:whipjawShred")
            {
                Name = "Whipjaw shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region spikePlantShred
            listOfEntityTypes.Add(new EntityType("item:spikePlantShred")
            {
                Name = "Spike plant shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region forestguardianShred
            listOfEntityTypes.Add(new EntityType("item:forestguardianShred")
            {
                Name = "Forest guardian shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region Bajingan shreds
            listOfEntityTypes.Add(new EntityType("item:thinThunderChickenShred")
            {
                Name = "Bajingan shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region Field quadite shreds
            listOfEntityTypes.Add(new EntityType("item:leafcutterShred")
            {
                Name = "Field quadite shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region Mud worm shreds
            listOfEntityTypes.Add(new EntityType("item:mudWormShred")
            {
                Name = "Mud worm shreds",
                SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
                Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
                ItemType = new ItemType()
                {
                    // don't set the maximum bulk, the animal will tear off as much as it can eat, and other creatures will be able to smaller amounts from the item, if needed.
                    HasNoMaximumBulk = true,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "fleshShreds",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #endregion
            #region binalRat flesh
            listOfEntityTypes.Add(new EntityType("item:binalRatChunk")
            {
                Name = "Binal rat flesh chunk",
                SummaryDescription = "A cut up binal rat", //process is butchering and getting 2 pieces.
                Description = "Can be used as bait, may even attract other binal rats. Binal rat meat is inedible by humans due to the high amount of toxins.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.5f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw meat",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fleshShred" } } } }
            });
            #endregion
            #region thunderChickenMeat
            listOfEntityTypes.Add(new EntityType("item:thunderChickenMeat")
            {
                Name = "Thunder chicken meat",
                SummaryDescription = "Various cuts from the planet's most tasty animal",
                Description = "Surprisingly dark meat.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.07f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "rawMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw meat",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } }}
            });
            #endregion
            #region thunderChickenguts
            listOfEntityTypes.Add(new EntityType("item:thunderChickenGuts")
            {
                Name = "Thunder chicken guts",
                SummaryDescription = "Offal to go in a stew.",
                ItemType = new ItemType()
                {

                    MaximumBulk = 0.05f, // Lars: only one guts item was yielded from a carcass???

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "rawMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gutsyellow" } } } }
            });
            #endregion 
            #region thunderChickenStew
            listOfEntityTypes.Add(new EntityType("item:thunderChickenStew")
            {
                Name = "Thunder chicken stew",
                SummaryDescription = "A modest campfire meal cooked in a pot",
                Description = "Main ingredient is thunder chicken offal. The cook adds whatever spices are at hand.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f, 
                        
                    FoodType = new FoodType()  
                    {
                        FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["meatSoup"],
                        IsMeal = true,
                        FoodTags = new string[] { "cookedMeat" }
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } }}
            });
            #endregion       
            #region thunderChickenSkewers
            listOfEntityTypes.Add(new EntityType("item:thunderChickenSkewers")
            {
                Name = "Chicken skewers",
                SummaryDescription = "Pieces of thunder chicken meat put on spits and roasted on the campfire",
                Description = "The cook mixes the meat pieces with whatever vegetables are at hand. Provides a good deal of recommended daily intake of protein and nutrients.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                        
                    FoodType = new FoodType()
                    {
                        FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
                        IsMeal = true,
                        FoodTags = new string[] { "cookedMeat" } 
                    }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } }}
            });
            #endregion
            //thunderChickenRoast  from carcass not possible because immovable.
            #region smokedThunderChicken
            listOfEntityTypes.Add(new EntityType("item:smokedThunderChicken")
            {
                Name = "Smoked thunder chicken",
                SummaryDescription = "Smoked meat that keeps for some days.", 
                Description = "This meat has been preserved by smoking, extending its shelf life when stored at room temperature. But it will not keep indefinitely.", //
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "somewhatPreserved", 
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } }}
            });
            #endregion
            #region driedThunderChicken
            listOfEntityTypes.Add(new EntityType("item:driedThunderChicken")
            {
                Name = "Dried thunder chicken",
                SummaryDescription = "Dried meat that keeps for several months.",
                Description = "This meat has been preserved by drying and salting and will not spoil easily.", //
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } } }
            });
            #endregion


            #region clamwich
            listOfEntityTypes.Add(new EntityType("item:clamwich") //"item:clamwichCleaned"
            {
                Name = "Clamwich",
                SummaryDescription = "Mollusc found on mudflats",  // http://en.wikipedia.org/wiki/Mudflat
                Description = "\n SURVIVAL GUIDE NOTES\n The clamwich offers some nutritional value but has harmful toxins and needs to be carefully cleaned before cooking. It's a time consuming process and a well-organized kitchen area is recommended.\n \n BIOLOGY OVERVIEW\n Through sheer evolutionary chance, the clamwich shares many characteristics of Earth-based clams, specifically the Northern Quahog. The clamwich feeds on plankton and other organic particles extracted from the water. Can be found where tides and rivers deposit muddy sediments.", 
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "smallRawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "clamwichPile" } } } }
            });
            #endregion
            #region clamwichSoup
            listOfEntityTypes.Add(new EntityType("item:clamwichSoup")
            {
                Name = "Clamwich soup",
                SummaryDescription = "Soup made of clamwich",
                Description = "Requires some boiling.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["meatSoup"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region torux
            listOfEntityTypes.Add(new EntityType("item:torux")
            {
                Name = "Torux",
                SummaryDescription = "Donut shaped mollusc found on beaches",  //http://en.wikipedia.org/wiki/Shellfish
                Description = "\n BIOLOGY OVERVIEW\n The peculiar shellfish found on saltwater shores are the second stage of the torux life cycle. \n When it washes up on the shore it anchors itself to the seabed, converting to a stationary life similar to a mussel.\n \nSURVIVAL GUIDE NOTES\n Edible after cooking.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "smallRawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "toruxPile" } } }}
            });
            #endregion
            #region bakedTorux
            listOfEntityTypes.Add(new EntityType("item:bakedTorux")
            {
                Name = "Baked torux",
                SummaryDescription = "Baked in their shells among the coals",
                Description = "The taste takes some getting used to, but it serves as a modest source of protein.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "toruxPile" } } }}
            });
            #endregion
            #region alabasterRay - FISH
            listOfEntityTypes.Add(new EntityType("item:alabasterRay")
            {
                Name = "Alabaster ray",
                SummaryDescription = "Aquatic, plated creature",
                Description = "\n BIOLOGY OVERVIEW\n Spends its life on the seabed, its flat body weighed down by heavy bony armor. It can be spotted in shallow coastal waters feeding on crustaceans which it dislodges and crushes with its strong jaws. It is reminiscent of the prehistoric plated fish that once were widespread in Earth's oceans.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "rawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } }}
            });
            #endregion
            #region roastedAlabasterRay
            listOfEntityTypes.Add(new EntityType("item:roastedAlabasterRay")
            {
                Name = "Roasted alabaster ray",
                SummaryDescription = "Pieces of alabaster meat roasted on long skewers",  //http://www.fieldandstream.com/answers/fishing/more-freshwater/other/how-do-you-cook-and-clean-snapping-turtles-anything-apreciated
                Description = "It takes some time to separate the flesh from the shell, but it is well worth the effort.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } },

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } }}
            });
            #endregion
            #region alabasterStew
            listOfEntityTypes.Add(new EntityType("item:alabasterStew")
            {
                Name = "Alabaster stew",
                SummaryDescription = "Alabaster ray left to simmer in a pot",  //can the shell be used as boiling vessel? http://en.wikipedia.org/wiki/Stew#History   http://www.fieldandstream.com/answers/fishing/more-freshwater/other/how-do-you-cook-and-clean-snapping-turtles-anything-apreciated
                Description = "Tender and juicy alabaster meat slowly heated in a pot with whatever spices and vegetables the cook chooses.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["meatSoup"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } },

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } }}
            });
            #endregion
            #region pickledAlabasterRay
            listOfEntityTypes.Add(new EntityType("item:pickledAlabasterRay")
            {
                Name = "Pickled alabaster ray",
                SummaryDescription = "Fried fish pickled in vinegar. Keeps for several months",
                Description = "This fish has been preserved by pickling (covered in vinegar) and will not spoil easily.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    RequiredStorageTags = new[] { "storageTagLiquidContainerClosedNoHeat" },

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "pickledFood",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } } }
            });
            #endregion
            #region smokedAlabasterRay
            listOfEntityTypes.Add(new EntityType("item:smokedAlabasterRay")
            {
                Name = "Smoked alabaster ray",
                SummaryDescription = "Smoked fish that keeps for some days",
                Description = "This fish has been preserved by smoking, extending its shelf life when stored at room temperature. But it will not keep indefinitely.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "somewhatPreserved",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } } }
            });
            #endregion
            #region streaKFin - FISH
            listOfEntityTypes.Add(new EntityType("item:streakFin")
            {
                Name = "Streak fin",
                SummaryDescription = "Fast-moving, saltwater predator. Catches flying insects.",
                Description = "Has some nutritional value. Can be caught with pig fly bait.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.07f, //mp apr 2015: if changing this, remember to update the same number in initializeFishTrap in actionsetsloader

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], FoodTags = new string[] { "rawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tentacles" } } }}
            });
            #endregion
            #region roastedStreakFin
            listOfEntityTypes.Add(new EntityType("item:roastedStreakFin")
            {
                Name = "Roasted streak fin",
                SummaryDescription = "Rather tangy tasting roasted fish.",
                Description = "The taste is best drowned out by campfire smoke and generous use of spices.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } },

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } }}
            });
            #endregion
            #region smokedStreakFin
            listOfEntityTypes.Add(new EntityType("item:smokedStreakFin")
            {
                Name = "Smoked streak fin",
                SummaryDescription = "Smoked fish that keeps for some days",
                Description = "This fish has been preserved by smoking, extending its shelf life when stored at room temperature. But it will not keep indefinitely.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "somewhatPreserved",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } } }
            });
            #endregion
            #region driedSaltedStreakFin
            listOfEntityTypes.Add(new EntityType("item:driedSaltedStreakFin")
            {
                Name = "Dried, salted streak fin",
                SummaryDescription = "Fish that has been salted and dried. Can keep for many months",
                Description = "Streak fin shares similarities with Earth's lean whitefish such as cod and is well suited for this preservation method. Thoroughly dried and salted it can be stored at room temperature for a long time. Inedible to humans and animals before the salt has been removed by soaking in water.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], FoodTags = new string[] { "inedibleIngredient" } }//no animals or humans want to eat it
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } } } //todo white
            });
            #endregion
            #region desalinatedStreakFin
            listOfEntityTypes.Add(new EntityType("item:desalinatedStreakFin")
            {
                Name = "Soaked streak fin",
                SummaryDescription = "Salted fish that has been soaked for some time and is ready to eat",
                Description = "Dried, salted streakfin requires that the salt be removed before it can be eaten. This is done by soaking it in water for some time.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } },//todo further cooking step is more realistic, but maybe too involved.

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } } }//todo white fillets
            });
            #endregion

            #region carbonTail - FISH
            listOfEntityTypes.Add(new EntityType("item:carbonTail")
            {
                Name = "Carbon tail",
                SummaryDescription = "Powerful swimmer that lurks among the water weeds",
                Description = "\n BIOLOGY OVERVIEW\n The creature feeds by ambushing passing fishes.\n \nSURVIVAL GUIDE NOTES\n It can be caught with patience and appropriate bait. A meal can then be prepared from it.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.07f, //mp apr 2015: if changing this, remember to update the same number in initializeFishTrap in actionsetsloader

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], FoodTags = new string[] { "rawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tentaclesPurple" } } }}
            });
            #endregion
            #region roastedCarbonTail
            listOfEntityTypes.Add(new EntityType("item:roastedCarbonTail")
            {
                Name = "Roasted carbon tail",
                SummaryDescription = "Carbon tail roasted in the coals",
                Description = "Very appetizing appearance and smell.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region smokedCarbonTail
            listOfEntityTypes.Add(new EntityType("item:smokedCarbonTail")
            {
                Name = "Smoked carbon tail",
                SummaryDescription = "Smoked fish that keeps for some days",
                Description = "This fish has been preserved by smoking, extending its shelf life when stored at room temperature. But it will not keep indefinitely.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "somewhatPreserved", 
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } } }
            });
            #endregion
            #region pickledCarbonTail
            listOfEntityTypes.Add(new EntityType("item:pickledCarbonTail")
            {
                Name = "Pickled carbon tail",
                SummaryDescription = "Fried fish pickled in vinegar. Keeps for several months",
                Description = "This fish has been preserved by pickling and will not spoil easily.", // Each piece provides a 3rd of recommended daily intake of protein.
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    RequiredStorageTags = new[] { "storageTagLiquidContainerClosedNoHeat" },

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "pickledFood",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatSmoked" } } } }
            });
            #endregion

            #region phantomWeaver
            listOfEntityTypes.Add(new EntityType("item:phantomWeaver")
            {
                Name = "Phantom weaver",
                SummaryDescription = "Small creature. Completely unknown.",
                Description = "This peculiar animal may or may not be fit to eat. There is abundant room for further research.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "rawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } }}
            });
            #endregion
            #region roastedPahntomWeaver
            listOfEntityTypes.Add(new EntityType("item:roastedPhantomWeaver")
            {
                Name = "Roasted phantom weaver",
                SummaryDescription = "Phantom weaver roasted on a skewer",
                Description = "Provides some amount of protein",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region webWing
            listOfEntityTypes.Add(new EntityType("item:webWing")
            {
                Name = "Web wing",
                SummaryDescription = "Elusive flyer. Virtually unknown.",
                Description = "\n BIOLOGY OVERVIEW\n The web wing's lifecycle has four stages, like many insectoid arthropods, consisting of an egg stage, a larva stage, a pupa stage, and an adult stage. With many predators taking advantage of the animal's vulnerability during its first three stages of life, actual sightings of a winged adult are rare. The web wing's name comes from the spiderweb pattern that adorns the creature's fragile, butterfly-like wings.", // We have conflicting theories about this critter due to the small number of observations. If thoroughly cooked it should be safe to eat.
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "smallRawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } }}
            });
            #endregion
            #region roastedWebWing
            listOfEntityTypes.Add(new EntityType("item:roastedWebWing")
            {
                Name = "Roasted web wing",
                SummaryDescription = "Web wing roasted on a skewer",
                Description = "Provides some amount of protein",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region crestedFoiler
            listOfEntityTypes.Add(new EntityType("item:crestedFoiler")
            {
                Name = "Crested foiler",
                SummaryDescription = "Small, beautifully colored herbivore.",
                Description = "\n BIOLOGY OVERVIEW\n Somewhere between avian and insectoid, the crested foiler is highly recognizable by the small feathery tuft located on the top of its head. Both males and females are brightly colored and armored, suggesting that the foiler's appearance is more likely to be a type of defensive camouflage or aposematism rather than for mate attraction. The small foiler is too heavy for long-term flight, though it has many other defensive mechanisms to make up for this deficit: a sharp-hooked beak, a set of clawed wings, and a piercing shriek which can disorient its predators.\n \nSURVIVAL GUIDE NOTES\n The crested foiler doesn't seem to be a good candidate as a food source as its body doesn't produce a significant amount of meat.",

                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "smallRawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } }}
            });
            #endregion
            #region roastedCrestedFoiler
            //roast on campfire: http://paleohacks.com/questions/94902/what-do-you-roast-over-a-campfire.html#axzz2mdR4Z77z
            listOfEntityTypes.Add(new EntityType("item:roastedCrestedFoiler")
            {
                Name = "Roasted foiler",
                SummaryDescription = "Crested foiler roasted on a skewer",
                Description = "Provides some amount of protein",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region goldenCenobite
            listOfEntityTypes.Add(new EntityType("item:goldenCenobite")
            {
                Name = "Golden cenobite",
                SummaryDescription = "Small reptilian predator",
                Description = "\n BIOLOGY OVERVIEW\n The elusive cenobite often lives a hermitic life, separating itself from other individuals of its own species save for the purpose of mating. The creature can be found in two colors, black and the more common golden shade. The golden cenobite appears to be less violent in nature compared to its cousin, but its venom is equally, if not more, potent.\n \nSURVIVAL GUIDE NOTES\n Though the cenobite's fangs don't appear to be well-suited to pierce human skin, anyone handling it should take caution, especially during food preparation.",// Seems to use venom for catching its prey. If we ever resort to eating it, it must be carefully cleaned and cooked.

                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "smallRawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "smallCarcass" } } }}
            });
            #endregion
            #region roastedGoldenCenobite
            listOfEntityTypes.Add(new EntityType("item:roastedGoldenCenobite")
            {
                Name = "Roasted cenobite",
                SummaryDescription = "Golden cenobite roasted on a skewer",
                Description = "Provides some amount of protein",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region treeScuttler
            listOfEntityTypes.Add(new EntityType("item:treeScuttler") //not used in any recipe
            {
                Name = "Tree scuttler",
                SummaryDescription = "An especially large, voracious species of scuttler bug",
                Description = "BIOLOGY OVERVIEW\n Resembling an arachnid, the tree scuttler prefers to make its home in the leaves of the spoak tree, but doesn't discriminate against other leafy trees and shrubs. Its nests are built of a thin silky material. The creature has an extremely elevated metabolism and one scuttler can consume all the leaves of a single spoak in only a month; a swarm of scuttlers are capable of clearing a single tree in a matter of days. \n \nSURVIVAL GUIDE NOTES\n It takes just one of these creatures to make short work of an improvised shelter unless the building materials have been treated with a suitable repellant.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "smallCarcass" } } }}
            });
            #endregion
            #region stinkpup
            listOfEntityTypes.Add(new EntityType("item:stinkpup")
            {
                Name = "Stinkpup",
                SummaryDescription = "Small, mostly nocturnal animal",
                Description = "Scampers about in the undergrowth hunting for smaller prey. When threatened, releases a pungent smell as a deterrent. The creature's movements are highly erratic and it is capable of jumping short distances.",
                ItemType = new ItemType() { MaximumBulk = 0.07f,
                                            FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "smallCarcass" } } } }
            });
            #endregion
            #region muckGrinder
            listOfEntityTypes.Add(new EntityType("item:muckGrinder")
            {
                Name = "Muck grinder",
                SummaryDescription = "Grotesque creature, truly alien in appearance.",
                Description = "\n BIOLOGY OVERVIEW\n The muck grinder's physical structure and behavior are unlike anything previously encountered. It appears to have some sort of tongue-like appendage used for filtering through decayed matter within great reserves of mud. The creature then grinds down the matter with a set of sharp teeth formed around the inside of its cylindrical body. The tongue may also serve to make the grinder locomotive.\n \nSURVIVAL GUIDE NOTES\n Its general appearance tends to evoke revulsion in humans, though with some nimble bladework and application of heat it could be edible.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "rawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "smallCarcass" } } }}
            });
            #endregion
            #region roastedMuckGrinder
            listOfEntityTypes.Add(new EntityType("item:roastedMuckGrinder")
            {
                Name = "Roasted muck grinder",
                SummaryDescription = "Wholly unappetizing dish",
                Description = "Taste, appearance and smell are uniformly bad.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            //old, not used:
            #region spriteSlug
            /* listOfEntityTypes.Add(new EntityType("item:spriteSlug")
            {
                Name = "Sprite slug",
                SummaryDescription = "Small, slug-like creature covered in iridescent spikes. Contains a stimulant",
                Description = "\n BIOLOGY OVERVIEW\n Flashes with colour to attract mates.\n \nSURVIVAL GUIDE NOTES\n Can be gathered if great care is taken not to touch its sharp spikes. After cooking it can be ingested not for its nutrients but for the chemical compounds that have a stimulating effect on humans.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "rawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                Category = GameData.Instance.AllItemCategories["ingredients"],
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tentacles" } } }}
            });*/
            #endregion

            #region spriteSlug //new: can now be eaten raw. no cooking.  stimulant.
            listOfEntityTypes.Add(new EntityType("item:spriteSlug")
            {
                Name = "Sprite slug",
                SummaryDescription = "Small, slug-like creature covered in iridescent spikes. Contains a stimulant, eaten raw.",
                Description = "\n BIOLOGY OVERVIEW\n Flashes with colour to attract mates.\n \nSURVIVAL GUIDE NOTES\n Can be eaten raw if great care is taken not to touch its sharp spikes. It contains chemical compounds that have a stimulating effect on humans.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } } //cookedMeat:  mp didnt want to make a whole new foodtag. just to tell the humans that they can eat it raw.

                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "perishable, no freeze",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tentacles" } } } }
            });
            #endregion
            #region roastedSpriteSlug //mp: not used. eaten raw now
/*
            listOfEntityTypes.Add(new EntityType("item:roastedSpriteSlug")
            {
                Name = "Roasted sprite slug. Provides a stimulant",
                SummaryDescription = "Not much remains of the creature apart from the chemical compounds which can now be ingested in a relatively safe manner. They have a stimulating effect.",
                Description = "Alternative cooking method recommended.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                Category = GameData.Instance.AllItemCategories["preparedFood"],
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });*/
            #endregion
            #region crazyDweller
            listOfEntityTypes.Add(new EntityType("item:crazyDweller")
            {
                Name = "Crazy dweller",
                SummaryDescription = "Lives in crevices between rocks and large trees",
                Description = "\n BIOLOGY OVERVIEW\n Must occasionally emerge from its hiding hole. On those occasions it uses erratic movement and behavior to confuse and evade its many predators.\n \nSURVIVAL GUIDE NOTES\n Hard to catch but will make a tasty meal after cooking",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "smallRawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "smallCarcass" } } }}
            });
            #endregion
            #region roastedCrazyDweller
            listOfEntityTypes.Add(new EntityType("item:roastedCrazyDweller")
            {
                Name = "Roasted crazy dweller",
                SummaryDescription = "Rather delicious.",
                Description = "Catching and cooking this creature is worth the effort.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region daggermouth - FISH
            listOfEntityTypes.Add(new EntityType("item:daggermouth")
            {
                Name = "Daggermouth",
                SummaryDescription = "Long-bodied marine predator with sharp teeth",
                Description = "\n BIOLOGY OVERVIEW\n Thin, long and vicious, this predator's diet consists mostly of smaller fish, though it has been known to attack land-based critters that stray too close to its home.\n \nSURVIVAL GUIDE NOTES\n The daggermouth is not a fish to handle haphazardly. Its razor-sharp teeth can pierce flesh easily and can make short work of netting. Collection of the daggermouth should only be undertaken through spear-fishing. With the right cooking it can become a nutritious meal.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.15f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "rawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "smallCarcass" } } }}
            });
            #endregion
            #region roastedDaggerMouth
            listOfEntityTypes.Add(new EntityType("item:roastedDaggermouth")
            {
                Name = "Cooked daggermouth",
                SummaryDescription = "Cooked directly in the coals",
                Description = "After cooking, its tough hide is cut open and the insides can be scooped out.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region impEel - FISH
            listOfEntityTypes.Add(new EntityType("item:impEel")
            {
                Name = "Imp eel",
                SummaryDescription = "Elusive, long-bodied swimmer",
                Description = "\n BIOLOGY OVERVIEW\n Creature that swims at great speed inside the muckroot canal system, hunting smaller fishes. Its enemy is the patrician which is able to detect its presence from outside the muckroot tube walls. The patrician will stand in wait at carefully chosen choke points, then stab the imp eel through the wall, injecting it with a powerful dissolvant and sucking up its insides.\n \nSURVIVAL GUIDE NOTES\n If any attempt is made to catch this creature, attention is advised. A patrician might appear to protect its hunting grounds.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.15f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "rawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tentacles" } } }}
            });
            #endregion
            #region roastedImpEel
            listOfEntityTypes.Add(new EntityType("item:roastedImpEel")
            {
                Name = "Roasted imp eel",
                SummaryDescription = "Roasted on a spit. Rather savory",
                Description = "Provides some amount of protein",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region scampBeetle
            listOfEntityTypes.Add(new EntityType("item:scampBeetle") //no recipe
            {
                Name = "Scamp beetle",
                SummaryDescription = "Winged bug the size of a hand",
                Description = "\n BIOLOGY OVERVIEW\n Little is known about this stage of the scamp's life cycle. The previous stage was a grub emerged from a quadite carcass.\n \nSURVIVAL GUIDE NOTES\n The adult scamp is an aggressive beetle that can inflict a nasty bite.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.06f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "smallRawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw meat"
                       
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "smallCarcass" } } }}
            });
            #endregion
            #region scampGrub
            listOfEntityTypes.Add(new EntityType("item:scampGrub")
            {
                Name = "Scamp grub", // http://en.wikipedia.org/wiki/Parasitoids
                SummaryDescription = "Larvae stage of the scamp beetle",
                Description = "\n BIOLOGY OVERVIEW\n Parasite found in some quadite species. Once the quadite dies, the parasites grow in size as they quickly devour the quadite's flesh, after which they turn on each other. A single grub survives the feeding frenzy. It will then transition into a scamp beetle. This stage in its life cycle is less understood.\n \nSURVIVAL GUIDE NOTES\n After cooking, the scamp grub is an excellent source of protein.",

                ItemType = new ItemType()
                {
                    MaximumBulk = 0.06f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "smallRawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:scampBeetle",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "smallCarcass" } } }}
            });
            #endregion
            #region grubGrub
            listOfEntityTypes.Add(new EntityType("item:grubGrub")
            {
                Name = "Grub grub",
                SummaryDescription = "Scamp grub roasted on a skewer",
                Description = "A good source of protein in the wilderness.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region ursinix
            listOfEntityTypes.Add(new EntityType("item:ursinix")
            {
                Name = "Ursinix",
                SummaryDescription = "Spherically shaped animal that lies in ambush, half buried.",
                Description = "ANATOMY: Many legs on a spiked, round body give the ursinix an appearance somewhere between the terran spider and sea urchin. Has the size of a watermelon. \n \nBEHAVIOR/HABITAT: Most species are found in coastal areas. Burrows in soft, wet sediments where it lies in ambush, its body half covered. Attracts prey by extending a multicolor and vibrating lure into the air. Its ability to imitate many sounds and movements makes it able to attract a diverse range of small animals, although the binal rat is probably its favorite prey. Once the prey is near, the ursinix quickly impales it with a barbed spike, injecting a paralyzing venom which acts within seconds. The prey is then dragged into the gaping maw of the ursinix.\n \nSURVIVAL GUIDE NOTES\n We have found the ursinix to be a valuable food source, but preparing it for cooking is a rather elaborate process. It is recommended to kill the animal with a spear, out of reach of its venomous spike. It must then be dug out and the poison gland carefully removed from its back. Then, cut the ursinix in half alongside the mouth, remove stomach contents and set the two halves out to dry under the sun. This last step is very important, as the UV rays will break down harmful toxins.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.17f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], FoodTags = new string[] { "rawMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gutspink" } } }}
            });
            #endregion
            #region cleanedUrsinix
            listOfEntityTypes.Add(new EntityType("item:cleanedUrsinix")
            {
                Name = "Ursinix (cleaned)",
                SummaryDescription = "Ursinix with poison gland and stomach contents removed",
                Description = "Ursinix, cut in two halves and cleaned. Before it can be used in cooking, it needs to be dried in the sun. UV rays will break down the toxins in its flesh.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], FoodTags = new string[] { "rawMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "sun drying",
                    DegradesTo = "item:driedUrsinix"
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gutspink" } } }}
            });
            #endregion
            #region driedUrsinix
            listOfEntityTypes.Add(new EntityType("item:driedUrsinix")
            {
                Name = "Ursinix (dried)",
                SummaryDescription = "Ursinix ready for cooking",
                Description = "With poison gland removed and toxins broken down by sun drying, the ursinix is ready to be cooked.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], FoodTags = new string[] { "rawMeat" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:rottenMeat"
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "brainsYellow" } } }}
            });
            #endregion
            #region grilledursinix
            listOfEntityTypes.Add(new EntityType("item:grilledUrsinix")
            {
                Name = "Grilled Ursinix",
                SummaryDescription = "Excellent nutritional value and sweet crab-like taste.",
                Description = "",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"], IsMeal = true, FoodTags = new string[] { "cookedMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } } }
            });
            #endregion
            #region commonoilTubers
            listOfEntityTypes.Add(new EntityType("item:commonOilTubers")
            {
                Name = "Common oil tubers",  //http://en.wikipedia.org/wiki/Tuber  http://www.foodsubs.com/Tubers.html
                SummaryDescription = "Potato-like vegetable with high fat content.",
                Description = "\n SURVIVAL GUIDE NOTES\n A valuable source of nutrition that grows in the firegrass biome. Hard to spot but usually found underneath firegrass. Only requires cooking to be made edible.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"], FoodTags = new string[] { "inedibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:rottenStaple",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "staple" } } }}
            });
            #endregion
            #region spottedoilTubers
            listOfEntityTypes.Add(new EntityType("item:spottedOilTubers")
            {
                Name = "Spotted oil tubers",  //http://en.wikipedia.org/wiki/Tuber  http://www.foodsubs.com/Tubers.html
                SummaryDescription = "This variant of the oil tuber is inedible by humans until we process it with a special enzyme",
                Description = "\n SURVIVAL GUIDE NOTES\n Grows in the firegrass biome. Hard to spot but usually found underneath firegrass. Before we can eat it, it requires processing with a specially engineered enzyme. Such an enzyme could be made with a standard field lab.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"], FoodTags = new string[] { "inedibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:rottenStaple",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "staple" } } } }
            });
            #endregion
            #region bakedCommonOilTubers
            listOfEntityTypes.Add(new EntityType("item:bakedCommonOilTubers")
            {
                Name = "Baked common oil tubers",
                SummaryDescription = "Very nourishing. Cooked among the coals.",
                Description = "The taste is described as a cross between hazelnuts and mushrooms",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBeige" } } }}
            });
            #endregion
            #region mashedCommonoilTubers
            listOfEntityTypes.Add(new EntityType("item:mashedCommonOilTubers") //
            {
                Name = "Mashed oil tubers",
                SummaryDescription = "A single portion of spuds boiled and mashed in a pot.",
                Description = "The cook adds whichever spices are available. This recipe leaves the tubers soft and tasty.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBeige" } } }}
            });
            #endregion
            #region mashedSpottedoilTubers
            listOfEntityTypes.Add(new EntityType("item:mashedSpottedOilTubers")
            {
                Name = "Spotted oil mash",
                SummaryDescription = "A single portion of spuds boiled and mashed in a pot.",
                Description = "The spotted oil tubers have first been processed with a special enzyme to make them edible. After that, the cook adds whichever spices are available. This recipe leaves the tubers soft and tasty.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBeige" } } } }
            });
            #endregion
            #region glassyCreeperPods - SEED
            listOfEntityTypes.Add(new EntityType("item:glassyCreeperPods")
            {
                Name = "Glassy creeper pods",  //http://en.wikipedia.org/wiki/Legume
                SummaryDescription = "Seeds of the Glassy creeper plant",
                Description = "These transparent, pea-like seeds are a modest source of nutrition when cooked. They should be fairly easy to grow as crops on a suitable patch of soil because the glassy creeper will vigorously spread on uncontested ground",

                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,
                        
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"], FoodTags = new string[] { "inedibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "perishable",
                    DegradesTo = "item:rottenStaple",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "vegetables" } } }}
            });
            #endregion
            #region glassyPorridge
            listOfEntityTypes.Add(new EntityType("item:glassyPorridge")
            {
                Name = "Glassy porridge",  //http://en.wikipedia.org/wiki/Porridge
                SummaryDescription = "Oddly transparent dish made from creeper pods.",
                Description = "To realize its potential, this rather insipid dish needs to be accompanied by vegetables or meats of the cook's choosing.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBeige" } } } }
            });
            #endregion
            #region hardtack (crackers)
            listOfEntityTypes.Add(new EntityType("item:hardtack")
            {
                Name = "Hardtack",  //
                SummaryDescription = "Very hard crackers that can keep for years",
                Description = "All moisture has been baked from this bread, resulting in a very long shelf life if stored dry. They consist of flour made from glassy creeper pods mixed with water",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "staple" } } } }
            });
            #endregion

         /*   #region Crispbread // not used now
            listOfEntityTypes.Add(new EntityType("item:crispbread")
            {
                Name = "Crispbread",  //
                SummaryDescription = "Dry, crispy bread that can keep for years",
                Description = "", //todo
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:spoiledMeal",
                },
                Category = GameData.Instance.AllItemCategories["preparedFood"],
                RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "staple" } } } }
            });
            #endregion*/

            #region crystalBerries - SEED
            listOfEntityTypes.Add(new EntityType("item:crystalBerries")
            {
                Name = "Crystal berries",  //http://en.wikipedia.org/wiki/Berry    http://en.wikipedia.org/wiki/Capsicum
                SummaryDescription = "Small, hollow fruits with a crisp transparent shell",
                Description = "Very high energy content. Should not be eaten directly because of the sharp pieces from its shell. The crystal shrub is a fairly robust plant and the berries could be grown as crops on a suitable plot of land.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                        
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["highEnergy"], FoodTags = new string[] { "inedibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "perishable",
                    DegradesTo = "item:rottenVegetables",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "vegetables" } } }}
            });
            #endregion
            #region powderedCrystalBerries
            listOfEntityTypes.Add(new EntityType("item:powderedCrystalBerries")
            {
                Name = "Powdered crystal berries",  //
                SummaryDescription = "Extremely sweet white powder",
                Description = "By grounding the crystal berries we get a white powder many times sweeter than sugar.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                        
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["highEnergy"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "perishable",
                    DegradesTo = "item:organicMatter",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } }}
            });
            #endregion
            #region minnowslive - FISH
            listOfEntityTypes.Add(new EntityType("item:minnowsLive")
            {
                Name = "Minnows",
                SummaryDescription = "Small fish",
                Description = "Could be used in a soup",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "smallRawMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "brainsPink" } } }}
            });
            #endregion
            #region rottenMeat
            listOfEntityTypes.Add(new EntityType("item:rottenMeat")
            {
                Name = "Rotten meat",
                SummaryDescription = "Meat that is decomposing and inedible by humans.",
                Description = "Depending on conditions, it will continue to decompose into organic matter",

                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,
                        
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorMeat"], FoodTags = new string[] {"rottenMeat"} }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "decomposedFood",
                    DegradesTo = "item:organicMatter"
                },
                CategoryKey = "waste",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mushSmallReddish" } } } }
            });
            #endregion
            #region rottenVegetables
            listOfEntityTypes.Add(new EntityType("item:rottenVegetables")
            {
                Name = "Rotten vegetables",
                SummaryDescription = "Vegetables that are decomposing and inedible by humans.",
                Description = "Depending on conditions, it will continue to decompose into organic matter.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorVegetables"], FoodTags = new string[] { "inedibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "decomposedFood",
                    DegradesTo = "item:organicMatter",
                },
                CategoryKey = "waste",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mushSmallGreenish" } } } }
            });
            #endregion
            #region rottenStaple
            listOfEntityTypes.Add(new EntityType("item:rottenStaple")
            {
                Name = "Moldy staple food",
                SummaryDescription = "Staple vegetable food that is decomposing and inedible by humans.",
                Description = "Staples (such as roots, tubers, seeds and grains) normally keep for some time but this is past its expiry date. Depending on conditions, it will continue to decompose into organic matter.",                     

                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorVegetables"], FoodTags = new string[] { "inedibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "decomposedFood",
                    DegradesTo = "item:organicMatter",
                },
                CategoryKey = "waste",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mushSmallYellowish" } } } }
            });
            #endregion
            #region spoiledMeal
            listOfEntityTypes.Add(new EntityType("item:spoiledMeal")
            {
                Name = "Spoiled meal", 
                SummaryDescription = "The disgusting remnants of a meal that was never eaten.",
                ItemType = new ItemType()
                {

                    MaximumBulk = 0.15f,
                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowWeightBalancedMeal"], FoodTags = new string[] { "spoiledMeal" } }, //"spoiledMeal" foodtag because it contains both rotten veg and meat
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "decomposedFood",
                    DegradesTo = "item:organicMatter"
                },
                CategoryKey = "waste",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealSpoiled" } } }}
            });
            #endregion

            #region crystal wine
            listOfEntityTypes.Add(new EntityType("item:crystalWine")
            {
                Name = "Crystal wine",  //  
                SummaryDescription = "Fruit wine made from fermented crystal berries. Could be distilled into brandy",
                Description = "Around 8% alcoholic content. Despite its elegant name, this is a rather plain beverage. Its main advantage is that it's easy to make once you have the necessary yeast.", //

                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f, //mp 1 person portion size, so they can drink it
                    RequiredStorageTags = new[] { "storageTagLiquidContainerClosedNoHeat" },
                    FoodType = new FoodType() { IsMeal = true, IsDrunk = true, Effects = new[] { "cheapAlcohol" }, FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"], FoodTags = new string[] { "alcoholicBeverage" } } //todo

                },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Comfort },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "pickledFood",
                    DegradesTo = "item:rottenStaple",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "vegetables" } } } }
            });
            #endregion
            #region Crystal brandy
            listOfEntityTypes.Add(new EntityType("item:crystalBrandy")
            {
                Name = "Crystal brandy",
                SummaryDescription = "High-proof (45%) spirit made from crystal berries", //the still could make 95% but that's for fuel
                Description = "A strong alcoholic beverage made by distillation of crystal wine.", 
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    RequiredStorageTags = new[] { "storageTagLiquidContainerClosedNoHeat" },
                    FoodType = new FoodType() { IsMeal = true, IsDrunk = true, Effects = new[] { "improvedAlcohol" }, FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"], FoodTags = new string[] { "alcoholicBeverage" } } //todo
                },
                TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Comfort }, //  "comfortMedium",
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "pickledFood",
                    DegradesTo = "item:spoiledMeal",//todo. should not actually degrade...
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } }
            });
            #endregion

            #region simCoffee
            listOfEntityTypes.Add(new EntityType("item:simCoffee")
            {
                Name = "Hot sim coffee",
                SummaryDescription = "Hot coffee made from simulated beans.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    FoodType = new FoodType() { IsMeal = true, IsDrunk = true, Effects = new[] { "caffeine" }, FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"], FoodTags = new string[] { "coffee" } }
                },
                TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Comfort },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "hotFood",
                    DegradesTo = "item:simCoffeeCold",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } }}
            });
            #endregion
            #region simCoffeeCold
            listOfEntityTypes.Add(new EntityType("item:simCoffeeCold")
            {
                Name = "Cold sim coffee",
                SummaryDescription = "Coffee that has gone cold. Made from simulated beans.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f,
                    FoodType = new FoodType() { IsMeal = true, IsDrunk = true, FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"], FoodTags = new string[] { "coffee" } }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                CategoryKey = "preparedFood",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } }}
            });
            #endregion
            #region simCoffeeBeans
            listOfEntityTypes.Add(new EntityType("item:simCoffeeBeans")
            {
                Name = "Sim coffee beans",
                SummaryDescription = "Coffee beans that were chemically produced rather than grown. Not as good as the real thing.",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.05f         
                },              
                TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Comfort },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:organicMatter",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } }}
            });
            #endregion
            #region spottedoilTuberEnzyme
            listOfEntityTypes.Add(new EntityType("item:spottedOilTuberEnzyme")
            {
                Name = "Spotted oil tuber enzyme",
                SummaryDescription = "Enzyme for making spotted oil tuber edible",
                Description = "This enzyme can be made using our field lab. First, a sample of the spotted oil tuber is analyzed and then we can synthesize a batch of enzymes to add to the oil tubers during the cooking process.\n Note that the enzyme needs to be used for cooking immediately since it quickly degrades and becomes useless.",
                ItemType = new ItemType() { MaximumBulk = 0.01f, },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "enzyme",
                    DegradesTo = "item:degradedEnzyme",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } } //using powercell graphics for now
            });
            #endregion
            #region blackpulpEnzyme
            listOfEntityTypes.Add(new EntityType("item:blackpulpEnzyme")
            {
                Name = "Blackpulp enzyme",
                SummaryDescription = "Enzyme for making the blackpulp fruit edible",
                Description = "This enzyme can be made using our field lab. First, a sample of the blackpulp is analyzed and then we can synthesize a batch of enzymes to add to the blackpulp during the cooking process.\n Note that the enzyme needs to be used for cooking immediately since it quickly degrades and becomes useless.",
                ItemType = new ItemType() { MaximumBulk = 0.01f, },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "enzyme",
                    DegradesTo = "item:degradedEnzyme",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } } //using powercell graphics for now
            });
            #endregion
            #region heaxapineLeavesEnzyme
            listOfEntityTypes.Add(new EntityType("item:hexapineLeavesEnzyme")
            {
                Name = "Hexapine enzyme",
                SummaryDescription = "Enzyme for making the hexapine leaves edible",
                Description = "This enzyme can be made using our field lab. First, a sample of the hexapine leaf is analyzed and then we can synthesize a batch of enzymes to add to the hexapine leaves during the cooking process.\n Be aware that the enzyme needs to be used for cooking immediately since it quickly degrades and becomes useless.",
                ItemType = new ItemType() { MaximumBulk = 0.01f, },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "enzyme",
                    DegradesTo = "item:degradedEnzyme",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } } 
            });
            #endregion
            #region degradedEnzyme
            listOfEntityTypes.Add(new EntityType("item:degradedEnzyme")
            {
                Name = "Degraded enzyme",
                SummaryDescription = "The enzyme is past its expiry date and is no longer of use.",
                Description = "Enzymes need to be used for cooking immediately since they quickly degrade and becomes useless.",
                ItemType = new ItemType() { MaximumBulk = 0.01f, },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "decomposedFood",
                    DegradesTo = "item:organicMatter",
                },
                CategoryKey = "waste",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mushSmallGreyish" } } } }
            });
            #endregion
            #region twinklerPheromone
            listOfEntityTypes.Add(new EntityType("item:twinklerPheromone")
            {
                Name = "Twinkler pheromone", //maybe allomone http://en.wikipedia.org/wiki/Allomone
                SummaryDescription = "Pheromone extracted from twinkler, to be used in a rat repellent contraption",
                Description = "From a dead twinkler, we can extract a chemical compound which the twinkler uses to signal with. Using the field lab, we can enhance its properties, making it sufficiently strong to ward off binal rats (and other animals) when placed in a special contraption.",
                ItemType = new ItemType() { MaximumBulk = 0.01f, },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "perishable",
                    DegradesTo = "item:degradedChemical",
                },
                CategoryKey = "ingredients",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } } 
            });
            #endregion
            #region degradedChemical
            listOfEntityTypes.Add(new EntityType("item:degradedChemical")
            {
                Name = "Degraded chemical",
                SummaryDescription = "This chemical compound is past its expiry date and is no longer of use.",
                Description = "Some chemicals have a limited shelf life and some need to be stored under very specific conditions.",
                ItemType = new ItemType() { MaximumBulk = 0.01f, },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "decomposedFood",
                    DegradesTo = "item:organicMatter", //hmmm. make it dirt?
                },
                CategoryKey = "waste",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mushSmallGreyish" } } } }
            });
            #endregion

            #region food not in use feb 2015
            //the below is not in use MP
            /*
            // food produced by spikeplant, only rats will consume it
            listOfEntityTypes.Add(new EntityType("item:SpikePlantRatBait")
            {
                Name = "Binal Rat Bait",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.15f,


                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"] }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "stored dry",
                    DegradesTo = "item:spoiledMeal",
                },
                Category = GameData.Instance.AllItemCategories["preparedFood"],
                RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealMeatstapleveg" } } } }
            });
            */
            /* mp not used because the improvised kitchen cannot help with this step
            listOfEntityTypes.Add(new EntityType("item:clamwich")
            {
                Name = "Clamwich (Uncleaned)",
                SummaryDescription = "Mollusc found on mudflats",  // http://en.wikipedia.org/wiki/Mudflat
                Description = "\n SURVIVAL GUIDE NOTES\n The clamwich offers nutritional value but has harmful toxins and needs to be carefully cleaned before cooking. It's a time consuming process without the right tools.\n \n BIOLOGY OVERVIEW\n Through sheer evolutionary chance, the clamwich shares many characteristics of Earth-based clams, specifically the Northern Quahog. The clamwich feeds on plankton and other organic particles extracted from the water. Can be found where tides and rivers deposit muddy sediments.", 
                //"\n BIOLOGY OVERVIEW\n Through sheer evolutionary chance, the clamwich shares many characteristics of Earth-based clams, specifically the Northern Quahog. The clamwich feeds on plankton and other organic particles extracted from the water. Can be found where tides and rivers deposit muddy sediments.\n \nSURVIVAL GUIDE NOTES\n The clamwich offers some nutritional value but does not cover our dietary needs over a longer period of time, so it's important to find other sources of food"
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.1f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"], FoodTags = new string[] { "inedibleMeat" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                Category = GameData.Instance.AllItemCategories["ingredients"],
                RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "clamwichPile" } } }}
            });
            */
            /* not needed at the moment
            listOfEntityTypes.Add(new EntityType("item:bakedSpottedOilTubers")
            {
                Name = "Baked spotted oil tubers",
                SummaryDescription = "Very nourishing. Cooked among the coals.",
                Description = "The taste is described as a cross between hazelnuts and mushrooms",
                ItemType = new ItemType()
                {
                    MaximumBulk = 0.125f,

                    FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"], IsMeal = true, FoodTags = new string[] { "edibleVegi" } }

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "cooked food",
                    DegradesTo = "item:spoiledMeal",
                },
                Category = GameData.Instance.AllItemCategories["preparedFood"],
                RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mealBlackzpacho" } } } }
            });
            */
            /*
            listOfEntityTypes.Add(new EntityType("item:minnowsDead") //not in use right now 26/5-14
            {
                Name = "Dead minnows",
                SummaryDescription = "Small fish",
                Description = "Could be used in a soup",
                ItemType = new ItemType()
                {
                    Bulk = 0.1f,
                    MayStoreInHome = false
                    //                Food = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorMeat"] },

                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 0f,
                    DegradeType = "raw seafood",
                    DegradesTo = "item:rottenMeat",
                },
                Category = GameData.Instance.AllItemCategories["ingredients"],
                RenderableType = new RenderableType() { Default = new StaticConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "brainsPink" } } }
            });
            */
            #endregion


            #endregion


                #region RAW MATERIALS

 

            #region Black powder weapon materials
            #region blackPowder
            listOfEntityTypes.Add(new EntityType("item:blackPowder")
            {
                Name = "Black powder",
                SummaryDescription = "Simple chemical explosive", //
                Description = "Because it is simple to make, this ancient type of explosive can be useful when no other options are available.", 
                ItemType = new ItemType() { MaximumBulk = 0.15f },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "wetDecay",
                    DegradesTo = "item:decayedBlackPowder",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "blackPowder" } } } } 
            });
            #endregion
            #region decayedBlackPowder
            listOfEntityTypes.Add(new EntityType("item:decayedBlackPowder")
            {
                Name = "Decayed black powder", //
                SummaryDescription = "No longer useful as an explosive", //
                Description = "This blackpowder has decayed because it was not stored in dry conditions.", //
                ItemType = new ItemType() { MaximumBulk = 0.15f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",
                },
                CategoryKey = "waste",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "blackPowder" } } } } 
            });
            #endregion
            #region goldBullet
            listOfEntityTypes.Add(new EntityType("item:goldBullet")
            {
                Name = "Bullets (rifle projectiles)", //mp we could also make them standardized so that they can be exchanged for the bullet in a rifle cartridge?
                SummaryDescription = "Rifled gold bullets. Part of the ammunition for a rifled firearm",
                Description = "Must be put in bag and carried together with a pouch of black powder. Gold is plentiful and can be used as a substitute for lead projectiles because it has an even higher density and malleability. The bullet will fit snugly in the grooves of a rifled barrel, making it spin during flight which increases accuracy. When making these bullets, precision is needed and they must be cast using a special bullet mold.", 
                ItemType = new ItemType() { MaximumBulk = 0.05f },
                TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Security },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt", //gold, famously, does not corrode (or degrade?)
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "canister" } } } } //todo. placeholder asset
            });
            #endregion
            #region blunderbussBalls
            listOfEntityTypes.Add(new EntityType("item:blunderbussBalls")
            {
                Name = "Ball shot (gold)",//too long: (smooth bore projectiles)   mp we could also make them standardized so that they can be exchanged for the shot in a shotgun cartridge?
                SummaryDescription = "Uneven gold projectiles. Part of the ammunition for a musket/musketoon",
                Description = "Can only be used with smooth bore firearms. Is put in a bag and carried together with a pouch of black powder. Because little precision is needed for manufacturing this ammunition, they can be made from gold nuggets that are simply heated and hammered in a smithy, without casting.",
                ItemType = new ItemType() { MaximumBulk = 0.05f },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt", //gold, famously, does not corrode (or degrade?)
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "canister" } } } } //todo. placeholder asset
            });
            #endregion

            #region saltpeter
            listOfEntityTypes.Add(new EntityType("item:saltpeter")
            {
                Name = "Saltpeter powder",
                SummaryDescription = "Saltpeter refined into a fine white powder", //
                Description = "The mineral can be used as a component in black powder.", //add used as fertilizer when this functionality is made
                ItemType = new ItemType() { MaximumBulk = 0.25f },
                TierOrArea = new TierOrArea() { Tier = "basic" }, //not sec, to allow for trade
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "saltpeterPowder" } } } }
            });
            #endregion

            #endregion

            #region ///metalworking

            #region gunBarrelUnbored
            listOfEntityTypes.Add(new EntityType("item:gunBarrelUnbored")
            {
                Name = "Gun barrel (unbored)",
                SummaryDescription = "Unfinished gun barrel made from wrought iron and steel",
                Description = "A long, irregular tube which needs to undergo boring before it can be used in a firearm.",
                ItemType = new ItemType() { MaximumBulk = 0.07f },
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gunBarrel" } } } } //
            });
            #endregion
            #region gunBarrelSmoothLong
            listOfEntityTypes.Add(new EntityType("item:gunBarrelSmoothLong")
            {
                Name = "Gun barrel (smooth, long)",
                SummaryDescription = "For use in a smooth bore firearm or modified further for use in other gun types.",
                Description = "Can be used in a musket. Can also be rifled for use in a rifle. If shortened and with a larger caliber bored, it can be used in a shotgun-type weapon.",
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                ItemType = new ItemType() { MaximumBulk = 0.07f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gunBarrel" } } } } //
            });
            #endregion
            #region gunBarrelSmoothShort
            listOfEntityTypes.Add(new EntityType("item:gunBarrelSmoothShort")
            {
                Name = "Gun barrel (smooth, short)",
                SummaryDescription = "For use in a short, smooth bore firearm such as a shotgun",
                Description = "The short barrel is useful for hunting in bush where a long barrel can be impractical.", //
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                ItemType = new ItemType() { MaximumBulk = 0.07f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gunBarrel" } } } } //
            });
            #endregion
            #region gunBarrelRifled
            listOfEntityTypes.Add(new EntityType("item:gunBarrelRifled")
            {
                Name = "Gun barrel (rifled)",
                SummaryDescription = "Gun barrel for use in a rifled firearm",
                Description = "The grooves inside the barrel will cause a rifle bullet to spin, stabilizing its flight and greatly increase its accuracy and range compared to a non-rifled barrel.",
                TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Security },
                ItemType = new ItemType() { MaximumBulk = 0.07f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gunBarrel" } } } } //
            });
            #endregion

            #region gunStock
            listOfEntityTypes.Add(new EntityType("item:gunStock")
            {
                Name = "Gun stock",
                SummaryDescription = "The part of a firearm which is held against the shoulder",
                Description = "Versatile enough that it can be used for different gun types.",
                TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                ItemType = new ItemType() { MaximumBulk = 0.04f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } } } } //todo. placeholder
            });
            #endregion

            #region anvil
            listOfEntityTypes.Add(new EntityType("item:anvil")
            {
                Name = "Anvil", // https://en.wikipedia.org/wiki/Anvil 
                SummaryDescription = "Iron tool used in metalworking as a surface for hammering",
                Description = "The anvil itself is made by forging together billets of wrought iron.",
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ItemType = new ItemType() { MaximumBulk = 0.9f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",//bso: ick a degrade type that will "never" degrade
                },
                CategoryKey = "rawMaterials", //bso: should it be listet as a tool even though it isn't one?
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "blackBox" } } } }
            });
            #endregion
            #region barClamps
            listOfEntityTypes.Add(new EntityType("item:barClamps") //http://www.fullchisel.com/blog/wp-content/uploads/2013/04/iron-bar-clamps-and-extension.jpg
            {
                Name = "Bar clamps (simple)", //  
                SummaryDescription = "Iron clamps for securing an object so that it can be worked on",
                Description = "When a threaded rod is unavailable for making a vise, these tools can be used for holding an item in place: Curved and straight iron rods that work together with a notched bar.",
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ItemType = new ItemType() { MaximumBulk = 0.14f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",//
                },
                CategoryKey = "rawMaterials", //mp: should it be listed as a tool even though it isn't one?
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } } //
            });
            #endregion

          /*  #region metalLathe
            listOfEntityTypes.Add(new EntityType("item:metalLathe")
            {
                Name = "Metal lathe", //  
                SummaryDescription = "Machine tool for turning metal objects. Needs to be set up with a power source.",
                Description = "The machine consists of numerous metal components and a belt drive made from leather. Before it's operational however, it must be set up in a machine shop either with an engine or a human power unit attached.",
            #endregion*/

            #region metalLatheComponents
            listOfEntityTypes.Add(new EntityType("item:metalLatheComponents")
            {
                Name = "Metal lathe components", //  
                SummaryDescription = "The metal parts of a lathe such as the frame, bed, saddle, wheels, gears, screws etc", //..spindle
                Description = "Carefully handmade from cast gold - all parts fit together perfectly after some modifications are made.", //wrought iron and steel and leather belt drive added in assembly process of lathe
                TierOrArea = new TierOrArea() { Tier = "medium" },
                ItemType = new ItemType() { MaximumBulk = 0.5f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",//
                },
                CategoryKey = "rawMaterials", //mp: should it be listed as a tool even though it isn't one?
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "castComponents" } } } } //todo
            });
            #endregion

            #region humanPowerUnitComponents
            listOfEntityTypes.Add(new EntityType("item:humanPowerUnitComponents")
            {
                Name = "Human power unit components", //  
                SummaryDescription = "The metal parts for the human power unit. Pedals, cranks, chain drive",
                Description = "Primarily made from cast gold.",
                TierOrArea = new TierOrArea() { Tier = "medium" },
                ItemType = new ItemType() { MaximumBulk = 0.75f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",//
                },
                CategoryKey = "rawMaterials", //mp: should it be listed as a tool even though it isn't one?
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "castComponents" } } } } //todo
            });
            #endregion

            #region humanPowerUnit
            listOfEntityTypes.Add(new EntityType("item:humanPowerUnit")
            {
                Name = "Human power unit", //  
                SummaryDescription = "Pedal device which enables the worker to power a machine tool using his leg muscles",
                Description = "Shares some similarities with a stationary bicycle: Consists of pedals, cranks and a chain drive - mounted on a sturdy frame.",
                TierOrArea = new TierOrArea() { Tier = "medium" },
                ItemType = new ItemType() { MaximumBulk = 0.75f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",//
                    PartKeys = new SerializableDictionary<string, int>() { { "item:humanPowerUnitComponents", 1 }, { "item:sticks", 2 } }
                },
                CategoryKey = "rawMaterials", //mp: should it be listed as a tool even though it isn't one?                
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "humanPowerUnit" } } } } //todo
            });
            #endregion

          /*  #region extrusionMachine
                Name = "Extrusion machine", //  mp used both for small products and continous long items.
                SummaryDescription = "Machine tool for making plastic and rubber products",
                Description = "This machine is an integral part of the polymer workshop. It works by pressing plastic or rubber feedstock through a heated die, thereby forming long, continous products (tubes and sheets). When using a mould, the machine can also make smaller items.",//todo
 
            #endregion*/

            #region extrusionMachineComponents
            listOfEntityTypes.Add(new EntityType("item:extrusionMachineComponents")
            {
                Name = "Extrusion machine components", //  
                SummaryDescription = "The metal parts for an extrusion machine.",
                Description = "Primarily made from cast gold. All parts fit together perfectly after some modifications are made.",
                TierOrArea = new TierOrArea() { Tier = "basic" }, //since it is used on headway tut. make sure that player can buy it from beginning
                ItemType = new ItemType() { MaximumBulk = 1f }, // 0.4f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",//
                },
                CategoryKey = "rawMaterials", //mp: should it be listed as a tool even though it isn't one?
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "castComponents" } } } } //
            });
            #endregion

            #region loomComponents
            listOfEntityTypes.Add(new EntityType("item:loomComponents")
            {
                Name = "Loom components", //  
                SummaryDescription = "Wooden parts for a loom.",
                Description = "Careful carpentry has produced these components which can be assembled into a loom.",
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ItemType = new ItemType() { MaximumBulk = 1f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",//
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponents" } } } } //mp bigger sprite than the small bushcraft one
            });
            #endregion

            #region goldSheet
            listOfEntityTypes.Add(new EntityType("item:goldSheet")
            {
                Name = "Gold sheet", //  
                SummaryDescription = "Thin plates of gold which can be shaped into tubes, pipes and other objects", //
                Description = "Sharing some of the properties of copper sheet. A skilled blacksmith or metalworker can bend, hammer or cut this metal in countless ways.", //wrought iron and steel and leather belt drive added in assembly process of lathe
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ItemType = new ItemType() { MaximumBulk = 0.1f }, //mass conservation
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",//
                },
                CategoryKey = "rawMaterials", //
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "castComponents" } } } } //todo
            });
            #endregion

            #region stillComponents
            listOfEntityTypes.Add(new EntityType("item:stillComponents")
            {
                Name = "Still components", //  mp we could divide into a gold boiler and gold pipes/tubes  when these items have other uses.
                SummaryDescription = "Boiler and pipes made from gold plate: Main components of a still", //
                Description = "Carefully handmade from cast gold - a still can easily be put together from these parts.", //
                TierOrArea = new TierOrArea() { Tier = "basic" },
                ItemType = new ItemType() { MaximumBulk = 0.9f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",//
                },
                CategoryKey = "rawMaterials", //mp: should it be listed as a tool even though it isn't one?
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "castComponents" } } } } //todo
            });
            #endregion

            #region gaskets  Rubber parts
            listOfEntityTypes.Add(new EntityType("item:gaskets")
            {
                Name = "Rubber parts", //  
                SummaryDescription = "Rubber components for use in machines and engines", //
                Description = "Tough, resistant products such as gaskets used for sealing a machine. Manufactured from marshcot sap which is a natural polymer superior to the natural rubber from Earth. The sap is first cleaned, then coagulated with a mild acid and left to dry. Then, it is heated and shaped while mixed with sulfur which causes the rubber to toughen (vulcanize).", //todo
                TierOrArea = new TierOrArea() { Tier = "basic" },//used in headway tut. take care.
                ItemType = new ItemType() { MaximumBulk = 0.05f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment",//
                },
                CategoryKey = "rawMaterials", //
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } } } //todo
            });
            #endregion

            #region bogOre
            listOfEntityTypes.Add(new EntityType("item:bogOre")
                {
                    Name = "Bog ore",
                    SummaryDescription = "Lumps of iron ore, easily harvested. Found in bogs, near basalt rocks.",
                    Description = "The ore appears as small lumps in bogs where water flows from nearby iron-rich mountains. The deposits are easy to harvest and can serve as a resource for small-scale metalworking",
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.75f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "dirt",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironOre" } } } }
                });
            #endregion


            #region Scandium ore  RareMetalore1
            listOfEntityTypes.Add(new EntityType("item:scandiumOre")
            {
                Name = "Scandium ore",//"RareMetal Ore1"   https://en.wikipedia.org/wiki/Scandium
                SummaryDescription = "These minerals have an unusually high concentration of the chemical element scandium",
                Description = "The high concentration of scandium in this ore makes it possible to refine the metal on site when using an advanced refiner installation.",
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                ItemType = new ItemType() { MaximumBulk = 0.75f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "soilWhite" } } } }
            });
            #endregion
            #region Terbium ore  RareMetalore2
            listOfEntityTypes.Add(new EntityType("item:terbiumOre")
            {
                Name = "Terbium ore",
                SummaryDescription = "The chemical element terbium is present in high amounts in these minerals",
                Description = "The high concentration of terbium makes it easy to refine the metal when using an advanced refiner installation.",
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                ItemType = new ItemType() { MaximumBulk = 0.75f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",
                },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "soilGrey" } } } }
            }); 
            #endregion
          
            #region goldOre
            listOfEntityTypes.Add(new EntityType("item:goldOre")
            {
                Name = "Gold ore",
                SummaryDescription = "Gold nuggets are common on this planet and are easily harvested",// "Nuggets of gold ore, easily harvested from loose mountain soil"
                Description = "Big lumps can often be found in loose mountain soil and sediments. They have a high purity but usually require smelting to remove impurities before the gold can be cast into objects. However, the nuggets can also be shaped into simple items through forging at lower temperatures.",// "Gold is plentiful on this planet and big lumps can often be found in loose soil and sediments."
                ItemType = new ItemType() { MaximumBulk = 0.75f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "goldOre" } } } } 
            });
            #endregion
            #region gold
            listOfEntityTypes.Add(new EntityType("item:gold")
            {
                Name = "Gold",
                SummaryDescription = "Rods of gold",//
                Description = "Gold is very common on Antheia so its physical properties can be used for any purpose without fretting about price: It is very malleable, has high density and corrosion resistance. This gold has been smelted into rods of high purity and can be used to cast high quality objects.",
                ItemType = new ItemType() { MaximumBulk = 0.1f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "goldSticks" } } } } //
            });
            #endregion
            #region roughBloomIron
            listOfEntityTypes.Add(new EntityType("item:roughBloomIron")
                {
                    Name = "Bloom iron",
                    SummaryDescription = "Clump of iron and slag, a product of primitive iron smelting",
                    Description = "An early step in primitive metalworking, this requires further work by the blacksmith before the iron is pure enough to be used.",
                    ItemType = new ItemType() { MaximumBulk = 0.1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "dirt",
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                  /*  SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"iron", 1f },
                          }
                    },*/
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironBloom" } } } } 
                });
            #endregion
            #region wroughtIron
            listOfEntityTypes.Add(new EntityType("item:wroughtIron")
                {
                    Name = "Wrought iron",
                    SummaryDescription = "Iron rods that have been shaped on the anvil by a blacksmith",
                    Description = "These iron products are ready to be shaped into their final form such as simple iron tools.",
                    ItemType = new ItemType() { MaximumBulk = 0.1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "dirt",
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironRods" } } } } 
                });
            #endregion
            #region Scandium (refined) rareMetal
            //NA MINING CAMP MATERIAL
            listOfEntityTypes.Add(new EntityType("item:scandium")
            {
                Name = "Scandium (refined)",
                SummaryDescription = "Soft, silvery metal with a yellow shade. Only used for high tech products",
                Description = "Scandium is called a rare-earth metal. This chemical element is required in the construction of the electromagnetic shields known as Project CANOPY.",
                ItemType = new ItemType() { MaximumBulk = 0.1f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",
                },
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "lightOrangePowder" } } } }
            }); 
            #endregion
            #region Terbium (refined)  rareMetal2
            //NA MINING CAMP MATERIAL
            listOfEntityTypes.Add(new EntityType("item:terbium")
            {
                Name = "Terbium (refined)",
                SummaryDescription = "Silvery-white metal which is very soft. Only used for high tech products",
                Description = "Terbium is called a rare-earth metal. This chemical element is required in the construction of the electromagnetic shields known as Project CANOPY.",
                ItemType = new ItemType() { MaximumBulk = 0.1f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",
                },
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "lightGreyBluePowder" } } } }
            }); 
            #endregion
           
            #region blisterSteel
            listOfEntityTypes.Add(new EntityType("item:blisterSteel")
            {
                Name = "Blister steel",
                SummaryDescription = "Steel rods made from wrought iron using a carburization process",
                Description = "Named for the characteristic small lumps that comes from the carburization method, where the wrought iron is heat treated together with charcoal in a kiln for some time.  This increases the carbon content of the iron, creating blister steel. It can then be further worked with a hammer to produce steel products.",//
                ItemType = new ItemType() { MaximumBulk = 0.1f },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "dirt",
                },
                TierOrArea = new TierOrArea() { Tier = "basic" },
                CategoryKey = "rawMaterials",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "steelRods" } } } }
            });
            #endregion


            #endregion

            #region guano
            listOfEntityTypes.Add(new EntityType("item:guano")
                {
                    Name = "Guano pile", //was "Pile of guano"
                    SummaryDescription = "Excrements from animals and birds that live in caves or arid areas",//was "A pile of unrefined saltpeter",
                    Description = "Because the manure has not been subjected to rain, it has a high content of phosphate, nitrogen and potassium which makes it useful as a fertilizer. Can also be used for extracting chemical compounds such as saltpeter", // //saltpeter: "Can typically be found as crystallized deposits on cave walls, primarily in arid environments.",
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.5f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "dirt",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "soilGrey" } } } } 
                });
                #endregion


                #region saltpeterSolution
                listOfEntityTypes.Add(new EntityType("item:saltpeterSolution")
                {
                    Name = "Saltpeter solution",
                    SummaryDescription = "Saltpeter which has been fully dissolved in water", //
                    Description = "The saltpeter is ready to be refined by boiling.", //
                    ItemType = new ItemType() { MaximumBulk = 0.6f, RequiredStorageTags = new[] { "storageTagLiquidContainerNoHeat" } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "dirt", //mp I thought about making it degrade back to...what? guano? when the water vaporates. but I dunno.
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mush" } } } } //
                });
                #endregion



                #region marshcotSap
                listOfEntityTypes.Add(new EntityType("item:marshcotSap")
                {
                    Name = "Marshcot sap",  // http://en.wikipedia.org/wiki/Wood_preservation  //  http://en.wikipedia.org/wiki/Latex
                    SummaryDescription = "Fluid extracted from marshcot plants. Can be turned into rubber",
                    Description = "This emulsion has many of the same qualities found in natural rubber from Earth.", //todo
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.06f, RequiredStorageTags = new[] { "storageTagLiquidContainerNoHeat" } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mush" } } } }
                });

                #endregion


                #region sulfurPowder
                listOfEntityTypes.Add(new EntityType("item:sulfurPowder")
                {
                    Name = "Sulfur powder",
                    SummaryDescription = "Small pile of refined sulfur", //
                    Description = "N/A", //placeholder txt
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.25f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "dirt",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "sulfurPowder" } } } } 
                });
                #endregion

                #region clay
                listOfEntityTypes.Add(new EntityType("item:clay")
                {
                    Name = "Clay", //was: Clay (pile)
                    SummaryDescription = "Fine-grained soil material useful for ceramics and simple construction",
                    Description = "N/A",
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.75f }, //too big if it yields only one jar. some items should be made in pairs..
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "soil_s" } } } }
                });
                #endregion
                #region wetMudBrick
                listOfEntityTypes.Add(new EntityType("item:wetMudBrick")
                {
                    Name = "Mudbricks (wet)",
                    SummaryDescription = "The mudbricks are still wet. They dry slowly on their own OR quickly in a kiln",
                    Description = " The bricks are a simple and cheap construction material for buildings. Can either be dried slowly in the sun or quickly in a kiln and can then be used for construction. Made of clay, sand and some plant material.",
                    ItemType = new ItemType() { MaximumBulk = 0.75f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "sun drying",
                        DegradesTo = "item:solidMudBrick"
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mudBricksWet" } } } }
                });
                #endregion
                #region solidMudBrick
                listOfEntityTypes.Add(new EntityType("item:solidMudBrick")
                {
                    Name = "Mudbricks",
                    SummaryDescription = "Brick made of clay, sand and some plant material. They have dried and are ready for use in construction.",
                    Description = "The bricks are a simple and cheap construction material for buildings.",
                    ItemType = new ItemType() { MaximumBulk = 0.75f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt", //maybe degrade quicker..
                        DegradesTo = "item:clay"
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mudBricksDry" } } } }
                });
                #endregion

                #region unfinishedFirebricks
                listOfEntityTypes.Add(new EntityType("item:unfinishedFirebricks")
                {
                    Name = "Firebricks (unfinished)", //mp next step is firebricks that can withstand 1500 C (for blast furnace/cast iron)
                    SummaryDescription = "Unfinished bricks made of a special mix of clay, designed to withstand temperatures up to 1100 C",
                    Description = "When the bricks have been fired in a kiln they can be used for structures that must withstand the high heat involved in melting metals such as gold. The material consists of a carefully selected mix of silicon sand and kaolin clay.", //For steel, (1500 C) they would need Alumina hydrate and 37% Kaolin clay
                    ItemType = new ItemType() { MaximumBulk = 0.75f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt", //maybe degrade quicker..
                        DegradesTo = "item:clay"
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mudBricksWet" } } } }
                });
                #endregion

                #region Firebricks
                listOfEntityTypes.Add(new EntityType("item:firebricks")
                {
                    Name = "Firebricks", //mp next step is firebricks that can withstand 1500 C (for blast furnace/cast iron)
                    SummaryDescription = "Bricks made of a special mix of clay, designed to withstand temperatures up to 1100 C",
                    Description = "Used for structures that must withstand the high heat involved in melting metals such as gold. The material consists of a carefully selected mix of silicon sand and kaolin clay.", //For steel, (1500 C) they would need Alumina hydrate and 37% Kaolin clay
                    ItemType = new ItemType() { MaximumBulk = 0.75f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt", //maybe degrade quicker..
                        DegradesTo = "item:clay"
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mudBricksDry" } } } }
                });
                #endregion


                #region Salt
                listOfEntityTypes.Add(new EntityType("item:salt")
                {
                    Name = "Salt pile", //was: "Pile of salt"
                    SummaryDescription = "Salt is vital for nutrition and many other purposes",
                    Description = "Salt is useful for preserving food, since most of the bacteria that cause decomposition of organic matter are sensitive to high salt concentrations, just like on Earth.",
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                    ItemType = new ItemType() { MaximumBulk = 0.125f },//small portion because of cooking. make sure that salt mining yields a lot.
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "saltpeterPowder" } } } }
                });

                #endregion
           

                #region paint
                listOfEntityTypes.Add(new EntityType("item:paint")
                {
                    Name = "Paint",
                    TierOrArea = new TierOrArea() { Tier = "basic"},
                    ItemType = new ItemType() { MaximumBulk = 0.3f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "stored dry",                       
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "drum" } } }}
                });
                #endregion

                #region acetylene
                listOfEntityTypes.Add(new EntityType("item:acetylene")
                {
                    Name = "Acetylene canister",
                    SummaryDescription = "Hydrocarbon input material for the molecular assembler",
                    Description = "This simple hydrogen and carbon molecule is the main input material for creating diamondoid structures using the molecular assembler.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType() { MaximumBulk = 0.15f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "canister" } } }}
                });
                #endregion

                #region ironCanister
                listOfEntityTypes.Add(new EntityType("item:ironCanister")
                {
                    Name = "Canister (iron)",
                    SummaryDescription = "Iron input material for the molecular assembler",
                    Description = "Iron is needed for some products made by the molecular assembler.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType() { MaximumBulk = 0.15f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "canister" } } }}
                });
                #endregion

                #region plastCrete
                listOfEntityTypes.Add(new EntityType("item:plastCrete")
                {
                    Name = "PlastCrete",
                    SummaryDescription = "Binder for a concrete-like building material.",
                    Description = "Combines with water and aggregate to form a building material that is strong and light. Contains specially designed particles that combine into a strong lattice, reinforcing the structure.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType() {  MaximumBulk = 0.5f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "cement" } } }}
                });
                #endregion

                #region diamondGlass
                listOfEntityTypes.Add(new EntityType("item:diamondGlass")
                {
                    Name = "Diamond glass",
                    SummaryDescription = "Extremely durable and lightweight transparent material",
                    Description = "Made from carbon with a molecular assembler.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType() {  MaximumBulk = 0.5f },                    
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "neverDegrades"
                    },

                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "scrapMetal" } } } }
                });
           
                #endregion

              
                #region Catamaran parts -Moved to scenarioloader Tut
                /*
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
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "catamaranItem" } } } }
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
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "catamaranMastSail" } } } }
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
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "strippedCatamaran" } } } }
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
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "catamaranMast" } } } }
                });*/
                #endregion
                #region crashed skimmer parts
                listOfEntityTypes.Add(new EntityType("item:strippedHull")
                {
                    Name = "Stripped aircraft hull",
                    SummaryDescription = "We've stripped the hull part of the aircraft wreck for any useful materials",
  //                  Description = "N/A",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    Icon = "typeIcon_strippedHull",
                    ItemType = new ItemType() { MaximumBulk = 5f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "strippedHull" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:strippedTail")
                {
                    Name = "Stripped aircraft tail",
                    SummaryDescription = "We've stripped the tail part of the aircraft wreck for any useful materials",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    Icon = "typeIcon_strippedTail",
                    ItemType = new ItemType() { MaximumBulk = 4f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "strippedTail" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:strippedEngineSide")
                {
                    Name = "Stripped aircraft rotor",
                    SummaryDescription = "We've stripped this aircraft rotor for any useful materials",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    Icon = "typeIcon_strippedEngineSide",
                    ItemType = new ItemType() { MaximumBulk = 3f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "strippedEngineSide" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:strippedEngineTop")
                {
                    Name = "Stripped aircraft rotor",
                    SummaryDescription = "We've stripped this aircraft rotor for any useful materials",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    Icon = "typeIcon_strippedEngineTop",
                    ItemType = new ItemType() { MaximumBulk = 3f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "strippedEngineTop" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:propellerDome")
                {
                    Name = "Prop spinner",  //from googling, I think this word is more common than dome
                    SummaryDescription = "An aluminum dome from the Skimmer's propeller hub",
                    Description = "A camp member has suggested using this component as a cooking pot.",  // Made of aluminum. Its concave shape means it can be used as an improvised vessel.
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.15f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.8f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "scrapMetal" } } }}
                });
                # endregion
                #region organic matter
                listOfEntityTypes.Add(new EntityType("item:organicMatter")
                {
                    Name = "Organic matter",
                    SummaryDescription = "Decomposed plant or animal material",
                    Description = "Organic compounds that have come from the remains of plants and animals and their waste products.\n It will continue to decompose and eventually enter the soil as humus.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() {HasNoMaximumBulk = true },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "biowaste"
                    },
                    CategoryKey = "waste",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "mushSmallGreyish" } } } }
                });
                #endregion
                #region organicFertilizer
                listOfEntityTypes.Add(new EntityType("item:organicFertilizer")
                {
                    Name = "Compost fertilizer",// mp if renaming, remember to change name in description in useOrganicFertilizer in processloader
                    SummaryDescription = "Organic fertilizer for a farm plot",//
                    Description = "Made from decomposed plant and animal matter", //
                    TierOrArea = new TierOrArea() { Tier = "medium" }, //not necessarily for food, could also affect cotton crops
                    ItemType = new ItemType() { MaximumBulk = 0.1f }, //small bulk amount to minimize the chance of using less items with a smaller bulk size for special actions where bulk can't be accountet for
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "biowaste"                      
                    },

                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "cement" } } } }
                });
                #endregion
                #region guanoFertilizer
                listOfEntityTypes.Add(new EntityType("item:guanoFertilizer")
                {
                    Name = "Guano fertilizer",//
                    SummaryDescription = "Organic fertilizer for a farm plot",//
                    Description = "Made from natural guano deposits, this is a more concentrated fertilizer than compost.", //
                    TierOrArea = new TierOrArea() { Tier = "medium" }, //not necessarily for food, could also affect cotton crops
                    ItemType = new ItemType() { MaximumBulk = 0.25f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt"
                    },

                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "cement" } } } }
                });
                #endregion
                #region Firegrass sod
                listOfEntityTypes.Add(new EntityType("item:firegrassSod")
                {
                    Name = "Firegrass sod",
                    SummaryDescription = "Pieces of firegrass turf",
                    Description = "Firegrass grows thick with entangled roots. It is possible to cut pieces and use them as bricks for simple structures.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.75f }, // 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt",
                        DegradesTo = "item:organicMatter",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "firegrassSod" } } }}
                });
                #endregion
                #region Sulfur blocks
                listOfEntityTypes.Add(new EntityType("item:sulfurBlocks")
                {
                    Name = "Sulfur blocks",
                    SummaryDescription = "Pieces of sulfur crystals",
                    Description = "Gathered from vulcanic deposits. This sulfur has a relatively high purity but could be refined.",
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.75f }, 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt"
                        
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "sulfurOre" } } }}
                });
                #endregion
                #region Sulfur smoke bomb
                listOfEntityTypes.Add(new EntityType("item:sulfurSmokeBomb")
                {
                    Name = "Sulfur smoke bomb",
                    SummaryDescription = "Will burn and release noxious gas",
                    Description = "This smoke bomb will release large amounts of sulfur dioxide which prevents respiration in most organisms. Only effective in a closed space. Might be able to clear out a quadite nest...let's see what happens.",
                    ItemType = new ItemType() { MaximumBulk = 0.5f },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"

                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "cement" } } }}
                });
                #endregion
                #region Varmint bomb
                listOfEntityTypes.Add(new EntityType("item:varmintBomb") 
                {
                    Name = "Varmint bomb", //
                    SummaryDescription = "Black powder bomb useful for destroying animal tunnels",//
                    Description = "Has sufficent explosive power to collapse the entrance area of an underground nest",
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry" //
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "canister" } } } } 
                });
                #endregion
                #region scrapMetal
                listOfEntityTypes.Add(new EntityType("item:scrapMetal")
                {
                    Name = "Scrap metal",
                    SummaryDescription = "Pieces of aluminum and carbon from aircraft hull", //was: "Pieces of aluminium and carbon from aircraft skin" ...mp: made it more general, so can be used in more scenarios
                    Description = "Broken-off pieces from a scrapped aircraft / vehicle. Mostly consist of a hard and strong carbon structure covered with a thin layer of aluminum.", //txt overwritten in scenario file to avoid mention skimmer
                    TierOrArea = new TierOrArea() { Tier = "survival" }, //mp you could also set it to advanced, it doesnt have any gameplay difference atm until we put it in trade on Great descent
                    ItemType = new ItemType() { MaximumBulk = 0.9f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "scrapMetal" } } }}
                });
                #endregion
                #region panelScraps
                listOfEntityTypes.Add(new EntityType("item:panelScraps")
                {
                    Name = "Panel scraps",  
                    SummaryDescription = "Thermoplastics/composite panels from aircraft/vehicle interior",
                    Description = "Pieces of interior panels, ceiling and floor stripped from the inside of an aircraft or vehicle. Made of composite materials and thermoplastics.", //scenario specific. overwrite in other scenarios. but I changed it to be more general.
                    TierOrArea = new TierOrArea() { Tier = "survival" }, //mp you could also set it to advanced, it doesnt have any gameplay difference atm until we put it in trade on Great descent
                    ItemType = new ItemType() { MaximumBulk = 0.75f }, 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "scrapMetal" } } }}
                });
                #endregion
                #region hydraulicsComponents NOT USED
         /*       listOfEntityTypes.Add(new EntityType("item:hydraulicsComponents")
                {
                    Name = "Hydraulics components", //mp not used now. because skimmer doesnt have hydraulics anymore
                    SummaryDescription = "Metal pumps, valves and tubing",
                    Description = "Pieces of the hydraulics system used to transfer power to the moving parts of a Skimmer aircraft such as the tiltrotors. Various tubes, fittings and hosing made from aluminium and corrosion-resistant steel.", //scenario specific. overwrite in other scenarios.

                });*/
                #endregion
                #region inactivatedFoodCoolerUnit TODO change to general description, make specific descr in twinkler scenario
                listOfEntityTypes.Add(new EntityType("item:inactivatedFoodCoolerUnit")
                {
                    Name = "Air condition unit (inactivated)",
                    SummaryDescription = "Air condition unit salvaged from an aircraft",
                    Description = "Still works. We could place it in a food cache to keep temperature at 5 degrees Celsius. Would have a battery life of 2-5 months depending on environment.",//scenario specific. overwrite in other scenarios.
                    TierOrArea = new TierOrArea() { Tier = "survival" }, //mp you could also set it to advanced, it doesnt have any gameplay difference atm until we put it in trade on Great descent, then set it to advanced
                    ItemType = new ItemType() { MaximumBulk = 0.15f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "battery" } } } }
                });
                #endregion
                #region activatedFoodCoolerUnit
                listOfEntityTypes.Add(new EntityType("item:activatedFoodCoolerUnit")
                {
                    Name = "Air condition unit (activated)",
                    SummaryDescription = "Air condition unit salvaged from an aircraft",
                    Description = "The device is activated and will keep the surrounding temperature at 5 degrees Celsius. Will run out of battery in 2-5 months depending on environment.",//scenario specific. overwrite in other scenarios.
                    TierOrArea = new TierOrArea() { Tier = "survival" }, //mp you could also set it to advanced, it doesnt have any gameplay difference atm until we put it in trade on Great descent, then set it to advanced
                    ItemType = new ItemType() {  MaximumBulk = 0.15f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "adequateConstruction"   //replace with a battery fuel system (like campfire) when it becomes more stable
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "battery" } } } }
                });
                #endregion
                #region seatCushions

                listOfEntityTypes.Add(new EntityType("item:seatCushions")
                {
                    Name = "Seat cushions",
                    SummaryDescription = "Cushions from aircraft/vehicle seats", //
   //                 Description = "N/A",
                    TierOrArea = new TierOrArea() { Tier = "survival" }, //mp you could also set it to advanced, it doesnt have any gameplay difference atm until we put it in trade on Great descent 
                    ItemType = new ItemType() { MaximumBulk = 0.75f }, 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "cloth" } } }}
                });

                #endregion
                #region textile


                listOfEntityTypes.Add(new EntityType("item:textile")
                {
                    Name = "Textile",
                    SummaryDescription = "Piece of woven fabric",//mp same
                    Description = "A sturdy versatile fabric suited for outdoor and indoor use.", //
                    //was:     but simplified it for use in all scenarios. mp june 2016                
                //    SummaryDescription = "Piece of permeable synthetic fabric",
                    //    Description = "Scraps of textile typically used in vehicle cabins and home interiors.", // also: "Can find use where a fine mesh fabric is needed which allows air and liquid to pass through."
                    TierOrArea = new TierOrArea() { Tier = "basic" },//because made with weaver workshop
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "cloth" } } }}
                });
                #endregion

                #region fiber
                listOfEntityTypes.Add(new EntityType("item:fiber")
                {
                    Name = "Fiber",
                    SummaryDescription = "Natural or synthetic fiber material",
                    Description = "This material can be spun and woven into textile", //
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "cloth" } } } }
                });
                #endregion

                #region cotton
                listOfEntityTypes.Add(new EntityType("item:cotton")
                {
                    Name = "Cotton",
                    SummaryDescription = "Natural fiber material which grows in a ball around the cottonseed",
                    Description = "The cotton plant was brought from Earth but later modified for a new climate by the first pioneers. The cotton fibers can be spun and woven into textile or string. The seeds are used for growing the next crop.", //
                    TierOrArea = new TierOrArea() { Tier = "basic" }, //general because used for both beds (comf) and  fish net (food)
                    ItemType = new ItemType() { MaximumBulk = 0.2f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "cotton" } } } }
                });
                #endregion

                #region sticks
                listOfEntityTypes.Add(new EntityType("item:sticks")
                {
                    Name = "Sticks",
                    SummaryDescription = "Various treelimbs and pieces of wood",
                    Description = "This wood is useful for constructing simple tools and shelter.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.3f }, 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "firewood" } } }}
                });
                #endregion
                #region thorns // NOT USED
       /*         listOfEntityTypes.Add(new EntityType("item:thorns")
                {
                    Name = "Thorns",
                    SummaryDescription = "A handful of pointy, sharp thorns",
                    Description = "Can be made into fishing hooks",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.05f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }}
                });*/
                #endregion
                #region flintRough

                listOfEntityTypes.Add(new EntityType("item:flintRough")
                {
                    Name = "Flint",
                    SummaryDescription = "A pile of flint of various sizes and shapes",
                    Description = "Flint is a stone found near chalk or limestone which is easy to shape for use in simple tools",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.15f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "flint" } } }}
                });
                #endregion
             
                #region smallTent



                listOfEntityTypes.Add(new EntityType("item:smallTent")
                {
                    Name = "Small tent", //  tent (packed)" not used because it appears in parts list MP
                    SummaryDescription = "Tent that offers good protection from wind and weather",
                    Description = "Part of the Tau Ceti survival kit. Its smart fabric that can stabilize temperature makes the tent comfortable in both warm and cold conditions.",
                    ItemType = new ItemType() { MaximumBulk = 0.25f },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Comfort },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tarp" } } }}
                });
                #endregion
                #region octagonalTent

                listOfEntityTypes.Add(new EntityType("item:octagonalTent")
                {
                    Name = "Octagonal tent", //  tent (packed)" not used because it appears in parts list MP
                    SummaryDescription = "Tent that offers good protection from wind and weather",
                    Description = "Part of the Tau Ceti survival kit. Its smart fabric that can stabilize temperature makes the tent comfortable in both warm and cold conditions.",
                    ItemType = new ItemType() { MaximumBulk = 0.5f },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Comfort },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tarp" } } }}
                });

                #endregion
                #region domeTent

                listOfEntityTypes.Add(new EntityType("item:domeTent")
                {
                    Name = "Dome tent", // "Dome tent (packed)" not used because it appears in parts list MP
                    SummaryDescription = "Tent that offers good protection from wind and weather",
                    Description = "Part of the Tau Ceti survival kit. Its smart fabric that can stabilize temperature makes the tent comfortable in both warm and cold conditions.",
                    ItemType = new ItemType() { MaximumBulk = 0.5f },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Comfort },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tarp" } } }}
                });
                #endregion
                #region thermalTarp

                listOfEntityTypes.Add(new EntityType("item:thermalTarp")
                {
                    Name = "Thermal tarp",
                    SummaryDescription = "Sophisticated textile that provides protection from wind and weather",
                    Description = "Part of the Tau Ceti survival kit. Primarily intended for use in improvised shelters. Its smart fabric that can stabilize temperature makes the tarp useful in both warm and cold conditions.",
                    ItemType = new ItemType() { MaximumBulk = 0.15f },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Comfort }, //the shelter made with this is survival tier
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry" //do NOT use "equipment" because it has a tooltip that says colonists will not do maintencance on an item. this can lead to confusion when this item is part of a fish trap structure which IS being mainatained
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tarp" } } } }
                });
                // TB FishTrap 19.03.2015
                #endregion
                #region FISHTRAPS///////
 

                #region fishTrapShore 'basket'
                listOfEntityTypes.Add(new EntityType("item:fishTrapBasket")
                {
                    Name = "Basket fish trap", //trying to avoid confusion with the structure name
                    SummaryDescription = "A simple portable fish trap designed for catching the 'carbon tail'",
                    Description = "This bottle shaped wicker trap can catch the fish species 'carbon tail' if placed in a suitable spot near its fresh water habitat.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.55f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry" //do NOT use "equipment" because it has a tooltip that says colonists will not do maintencance on an item. this can lead to confusion when this item is part of a fish trap structure which IS being mainatained
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fishTrapCylinderSmall" } } } }

                });
                    #endregion
                #region fishTrapHoopNet
                listOfEntityTypes.Add(new EntityType("item:fishTrapHoopNet")
                {
                    Name = "Hoop net",//trying to avoid confusion with the structure name
                    SummaryDescription = "An good quality fish trap made from hoops and netting",
                    Description = "This cylindrical trap is efficient at catching the fish species 'carbon tail' if placed in a suitable spot near its fresh water habitat.",
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                    ItemType = new ItemType() { MaximumBulk = 0.65f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry" //do NOT use "equipment" because it has a tooltip that says colonists will not do maintencance on an item. this can lead to confusion when this item is part of a fish trap structure which IS being mainatained
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fishTrapHoopNetItem" } } } } 

                });
                #endregion
                #region fishingNet
                listOfEntityTypes.Add(new EntityType("item:fishingNet")
                {
                    Name = "Fishing net",
                    SummaryDescription = "A cotton mesh used for fish traps. Treated with a resin",
                    Description = "An experienced fisher can weave a good quality cotton net with a few simple tools. To protect the net from rotting it is treated with a resin.",
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                    ItemType = new ItemType() { MaximumBulk = 0.30f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry" //do NOT use "equipment" because it has a tooltip that says colonists will not do maintencance on an item. this can lead to confusion when this item is part of a fish trap structure which IS being mainatained
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "fishingNet" } } } } 

                });
                #endregion

                #endregion
                #region daysheenLeaves
                listOfEntityTypes.Add(new EntityType("item:daysheenLeaves")
                {
                    Name = "Daysheen leaves",
                    SummaryDescription = "Stiff, shiny, brightly coloured leaves",
                    Description = "The leaves naturally form a cone. We can take advantage of this property when building a shelter of such a shape.",
                    TierOrArea = new TierOrArea() { Tier = "survival"},
                    ItemType = new ItemType() { MaximumBulk = 0.3f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "proneToInfestation",
                        DegradesTo = "item:infestedLeavesRemains",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "daysheenLeaves" } } }}  //"daysheenLeaves"
                });
                #endregion
                #region charcoal
                listOfEntityTypes.Add(new EntityType("item:charcoal")
                {
                    Name = "Charcoal",
                    SummaryDescription = "A traditional fuel type made from heat treated organic matter", 
                    Description = "Charcoal can provide a more intense heat than firewood and is often required for primitive metalworking.", //
                    ItemType = new ItemType() { MaximumBulk = 0.3f, FuelType = new FuelType() {  } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "charcoal" } } } } //
                });
                #endregion
                #region firewood
                listOfEntityTypes.Add(new EntityType("item:firewood")
                {
                    Name = "Firewood",
                    SummaryDescription = "Kindling for a fire",
                    Description = "Dry plant matter such as twigs, leaves and branches. A basic fuel type required for many types of production.",
                    ItemType = new ItemType() { MaximumBulk = 0.3f /*Lars: was: 0.5f*/, FuelType = new FuelType() { FuelTags = new[] { "fuelForCampfire", "fuelForFieldKitchen" } } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "firewood" } } }}
                });
                #endregion
                #region wetFirewood
                listOfEntityTypes.Add(new EntityType("item:wetFirewood")
                {
                    Name = "Firewood (wet)",
                    SummaryDescription = "Fresh wood that needs to dry before being used as fuel",//todo
                    Description = "Can be placed in a woodpile to accelerate drying.",//todo
                    TierOrArea = new TierOrArea() { Tier = "survival" }, 
                    ItemType = new ItemType() { MaximumBulk = 0.3f /* Lars: was: 0.5f*/ },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "sun drying",//todo
                        DegradesTo = "item:firewood"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "firewood" } } } }
                });
                #endregion
                #region peat
                listOfEntityTypes.Add(new EntityType("item:dryPeat")
                {
                    Name = "Peat",
                    SummaryDescription = "Fuel. Dry slabs of partially decayed plant matter", //https://en.wikipedia.org/wiki/Peat
                    Description = "A type of fuel which can be extracted in bogs and marshes, locations where trees and therefore firewood is scarce. Its properties are quite similar to firewood.",
                    ItemType = new ItemType() { MaximumBulk = 0.3f , FuelType = new FuelType() { FuelTags = new[] { "fuelForCampfire" } } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "peatDry" } } } }
                });
                #endregion
                #region wetPeat
                listOfEntityTypes.Add(new EntityType("item:wetPeat")
                {
                    Name = "Peat (wet)",
                    SummaryDescription = "Wet slabs of partially decayed plant matter. After drying, used as fuel",//
                    Description = "When this has been dried, its properties are quite similar to firewood. Peat can be extracted in locations where trees and firewood is scarce. ",//
                    ItemType = new ItemType() { MaximumBulk = 0.3f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt", //doesnt dry in the sun in  a stockpile etc.
                        
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "peatWet" } } } }
                });
                #endregion


                #region waterCaneStem
                listOfEntityTypes.Add(new EntityType("item:waterCaneStem")
                {
                    Name = "Water cane stems",
                    SummaryDescription = "Stiff, bamboo-like tubes",
                    Description = "Can be useful for making various items that depend on these properties.",
                    ItemType = new ItemType() { MaximumBulk = 0.25f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "pole" } } }}
                });
                #endregion
                #region waterCaneLeaves
                listOfEntityTypes.Add(new EntityType("item:waterCaneLeaves")
                {
                    Name = "Water cane leaves",
                    SummaryDescription = "Tiny, stiff leaves useful as arrow fletchings",
                    Description = "This feather-like material can be carefully applied to the ends of arrows to stabilize their flight.",
                    ItemType = new ItemType() { MaximumBulk = 0.02f }, //changed from 0.05 by af 22/5-14
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wingweedLeaves" } } }}
                });

                #endregion
                #region giantHollowBud
                listOfEntityTypes.Add(new EntityType("item:giantHollowBud") //http://en.wikipedia.org/wiki/Bud
                {
                    Name = "Bowl-shaped flower bud",
                    SummaryDescription = "A young outgrowth from the 'giant hollow' plant",
                    Description = "The bud has the size of a bowl and is protected by tough scales. Its size and toughness could make it useful as an improvised food container. It simply needs to be hollowed out.", //\n BIOLOGY OVERVIEW\n The bud has the size of a bowl and is protected by tough scales. It will develop into a new plant when the right weather/environmental conditions are present.\n \n SURVIVAL GUIDE NOTES\n Its size and toughness could make it useful as an improvised food container. It simply needs to be hollowed out.",
                    ItemType = new ItemType() { MaximumBulk = 0.2f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "woodenPot" } } } }
                });
                #endregion
                #region shadeleafCanes
                listOfEntityTypes.Add(new EntityType("item:shadeleafCanes")
                {
                    Name = "Shadeleaf canes",
                    SummaryDescription = "Bendy, tough saplings",
                    Description = "The elastic limbs of a Shadeleaf tree are useful for construction of dome-shaped shelters and tools that need flexibility.",
                    ItemType = new ItemType() { MaximumBulk = 0.2f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "roughCanes" } } }}
                });
                #endregion
                #region shadeleafBowStave
                listOfEntityTypes.Add(new EntityType("item:shadeleafBowStave")
                {
                    Name = "Shadeleaf bow stave",
                    SummaryDescription = "A branch well-suited for making a bow",
                    Description = "This branch has the necessary properties for making a bow.",
                    ItemType = new ItemType() { MaximumBulk = 0.05f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bowStave" } } }}
                });
                #endregion
                #region wingweedLeaves
                listOfEntityTypes.Add(new EntityType("item:wingweedLeaves")
                {
                    Name = "Wingweed leaves",
                    SummaryDescription = "Large soft leaves",
                    Description = "Can be used for insulating shelters. However, they are prone to infestation by 'scuttlers' (a type of bug that inhabits the firegrass biome).",
                    ItemType = new ItemType() { MaximumBulk = 0.3f }, //AF 6/2-14 changed bulk from 0.5f
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "proneToInfestation",
                        DegradesTo = "item:infestedLeavesRemains",
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wingweedLeaves" } } }}
                });

                #endregion

                #region wingweedMats
                listOfEntityTypes.Add(new EntityType("item:wingweedMat")
                {
                    Name = "Wingweed mat", 
                    SummaryDescription = "Leaf mat for insulating shelters",
                    Description = "With some patience, wingweed leaves can be made into mats which will increase the comfort of a shelter. The leaves are treated with a preservative which increases their resistance from bug infestation.",
                    ItemType = new ItemType() { MaximumBulk = 0.5f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wingweedLeaves" } } }}
                });

                #endregion

                #region wingweed mat upgrade

                string matEffect = "mats";
                string matDescription = "The mats increase the comfort of a shelter. The leaves are treated with a preservative which increases their resistance from bug infestation.";
                listOfEntityTypes.Add(new EntityType("item:wingweedMats1People")
                {
                    Name = "Wingweed mats (1)",
                    SummaryDescription = "Upgrade: 1 leaf mat for shelters",
                    Description = matDescription,
                    ItemType = new ItemType() { MaximumBulk = 0.5f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        SalvageProcess = "salvageWingweedMats1People",
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:wingweedMat", 1 }
                        },
                        Repair = "buildingRepair"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "mats1People" },
                        Effects = new[] { matEffect },
                        SpriteModifier = StateModifier.Upgrade1
                    },
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wingweedLeaves" } } } }
                });

                listOfEntityTypes.Add(new EntityType("item:wingweedMats2People")
                {
                    Name = "Wingweed mats (2)",
                    SummaryDescription = "Upgrade: 2 leaf mats for shelters",
                    Description = matDescription,
                    ItemType = new ItemType() { MaximumBulk = 1f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        SalvageProcess = "salvageWingweedMats2People",
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:wingweedMat", 2 }
                        },
                        Repair = "buildingRepair"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "mats2People" },
                        Effects = new[] { matEffect },
                        SpriteModifier = StateModifier.Upgrade1
                    },
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wingweedLeaves" } } } }
                }); 
            
                listOfEntityTypes.Add(new EntityType("item:wingweedMats3People")
                {
                    Name = "Wingweed mats (3)",
                    SummaryDescription = "Upgrade: 3 leaf mats for shelters",
                    Description = matDescription,
                    ItemType = new ItemType() { MaximumBulk = 1.5f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        SalvageProcess = "salvageWingweedMats3People",
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:wingweedMat", 3 }
                        },
                        Repair = "buildingRepair"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "mats3People", "bedsOrMats3People" },
                        Effects = new[] { matEffect },
                        SpriteModifier = StateModifier.Upgrade1
                    },
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wingweedLeaves" } } } }
                });

                listOfEntityTypes.Add(new EntityType("item:wingweedMats4People")
                {
                    Name = "Wingweed mats (4)",
                    SummaryDescription = "Upgrade: 4 leaf mats for shelters",
                    Description = matDescription,
                    ItemType = new ItemType() { MaximumBulk = 2f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        SalvageProcess = "salvageWingweedMats4People",
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:wingweedMat", 4 }
                        },
                        Repair = "buildingRepair"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Comfort },
                    
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "mats4People", "bedsOrMats4People" },
                        Effects = new[] { matEffect },
                        SpriteModifier = StateModifier.Upgrade1
                    },                   
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wingweedLeaves" } } } }
                });

                #endregion

                #region bed
                listOfEntityTypes.Add(new EntityType("item:bedFrame")
                {
                    Name = "Bed frame",
                    SummaryDescription = "Sturdy wooden bed frame, made from naturally shaped materials",
                    Description = "This bed frame was made by exploiting the curves in the spoak tree branches.",                    
                    ItemType = new ItemType() { MaximumBulk = 1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Comfort },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "furniture" } } } }
                });

                #endregion

                #region bed upgrade
                string[] bedEffects = new[] { "simpleBed" };
                string bedUpgradeDescription = "These beds will provide increased comfort to the residents.";

                listOfEntityTypes.Add(new EntityType("item:beds3People")
                {
                    Name = "Beds (3)",
                    SummaryDescription = "Upgrade: 3 simple wooden beds for a home",
                    Description = bedUpgradeDescription,
                    ThumbnailSmall = "HUD_thumbnail_placeholder",
                    ItemType = new ItemType() { MaximumBulk = 4f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageBeds3People",
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:bedFrame", 3 },
                            { "item:textile", 3 }
                        },
                        Repair = "buildingRepair"
                    },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "bedsOrMats3People" },
                        Effects = bedEffects,
                        SpriteModifier = StateModifier.Upgrade2
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Comfort },
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "furniture" } } } }
                });

                listOfEntityTypes.Add(new EntityType("item:beds4People")
                {
                    Name = "Beds (4)",
                    SummaryDescription = "Upgrade: 4 simple wooden beds for a home",
                    Description = bedUpgradeDescription,
                    ThumbnailSmall = "HUD_thumbnail_placeholder",
                    ItemType = new ItemType() { MaximumBulk = 4f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                      DegradeType = "equipment",
                        SalvageProcess = "salvageBeds4People",
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                           { "item:bedFrame", 4 },
                           { "item:textile", 4 }
                        },
                        Repair = "buildingRepair"
                    },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "bedsOrMats4People" },
                        Effects = bedEffects,
                        SpriteModifier = StateModifier.Upgrade2
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Comfort },
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "furniture" } } } }
                });

                #endregion

                #region furniture
                listOfEntityTypes.Add(new EntityType("item:furniture")
                {
                    Name = "Furniture",
                    SummaryDescription = "Compact and functional table, chairs and shelves",
                    Description = "This furniture is beautifully made from the best spoak wood.",
                    ItemType = new ItemType() { MaximumBulk = 1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Comfort },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "furniture" } } } }
                });

                #endregion

                #region furniture upgrade
                string[] furnitureEffects = new[] { "furnitureEffect" };

                listOfEntityTypes.Add(new EntityType("item:furniture4People")
                {
                    Name = "Furniture (4)",
                    SummaryDescription = "Upgrade: Furniture for a small hut with 4 people",
                    Description = "Table, chairs and shelves which will provide increased comfort in a small hut",
                    ThumbnailSmall = "HUD_thumbnail_placeholder",
                    ItemType = new ItemType() { MaximumBulk = 1f }, //because only one "item:furniture" unit is used
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageFurniture4People",
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                           { "item:furniture", 1 },
                        },
                        Repair = "buildingRepair"
                    },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "furniture4People" },
                        Effects = furnitureEffects,
                        SpriteModifier = StateModifier.Upgrade2 //todo
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Comfort },
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "furniture" } } } }
                });

                #endregion

                #region Weaver upgrade

                listOfEntityTypes.Add(new EntityType("item:textileWorkshopUpgrade")
                {
                    Name = "Textile workshop",
                    SummaryDescription = "Upgrade: Simple machines for spinning fibers and weaving",
                    Description = "Equipped with a spinning wheel which spins thread from natural fibers. Also has a wooden loom which is used to weave cloth from thread.", // Color can be added to the textiles using the vats.
                    ThumbnailSmall = "HUD_thumbnail_workshopTextile",
                    ItemType = new ItemType() { MaximumBulk = 6f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "sturdyConstruction", // "equipment",
                        SalvageProcess = "salvageTextileWorkshopUpgrade", 
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:loomComponents", 1 }, 
                            { "item:waterCaneStem", 4 }                           
                        },
                        Repair = "buildingRepair" 
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "workshop" },
                        SpriteModifier = StateModifier.Upgrade3,
                        StorageSettings = "textileWorkshopStorage"
                    },

                    ToolType = new ToolType()
                    {
                        Durability = ItemLoader.toolDurabilityUnbreakable,
                        ToolHandling = ToolHandlingType.Stationary
                    },

                    ContainerType = new ToolContainerType() // use this to hold output
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee firewood inside                  
                        ProductionOutputStorageType = new ItemStorageType(2f),

                       // StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)

                    },

                    RenderableType = new RenderableType() // should never be seen...
                    {
                        DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }
                    }
                });
                #endregion

                #region Metalshop upgrade

                listOfEntityTypes.Add(new EntityType("item:metalLatheShopHumanPoweredUpgrade")
                {
                    Name = "Machine shop (human powered)",
                    SummaryDescription = "Upgrade: A work area for turning and making cylindrical metal shapes",
                    Description = "The workshop is built around a lathe: An essential tool for making precision-shaped metal items. The tool is powered by the operator's muscles through a simple gearbox.", //
                    ThumbnailSmall = "HUD_thumbnail_workshopMachinist",
                    ItemType = new ItemType() { MaximumBulk = 6f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],

                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "sturdyConstruction", //
                        SalvageProcess = "salvageMetalLatheShopHumanPoweredUpgrade",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:metalLatheComponents", 1 }, { "item:humanPowerUnit", 1 } }, 
                        Repair = "buildingRepair" 
                       
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium" }, //
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "workshop" },
                        SpriteModifier = StateModifier.Upgrade4,
                        StorageSettings = "metalWorkshopStorage"
                    },
                    ToolType = new ToolType()
                    {                        
                        Durability = ItemLoader.toolDurabilityUnbreakable,
                        ToolHandling = ToolHandlingType.Stationary
                    },
                    ContainerType = new ToolContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee firewood inside                  
                        ProductionOutputStorageType = new ItemStorageType(2f),
                        StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)
                      
                      //  DefaultStorageSettings = "metalShopStorage"
                    },                   
                    RenderableType = new RenderableType() // should never be seen...
                    {
                        DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }
                    }
                });

                #endregion

                #region Carpenter upgrade

                listOfEntityTypes.Add(new EntityType("item:carpenterWorkshopUpgrade")
                {
                    Name = "Carpenter's workshop",
                    SummaryDescription = "Upgrade: A well-equipped woodworking shop",
                    Description = "The workshop is equipped with tools for shaping the wood types that are common here, such as the curving spoak branches and the long straight tubes of the watercane. \nImportant assets are the foot powered spring pole lathe and the workbench.",
                    ThumbnailSmall = "HUD_thumbnail_workshopCarpenter",
                    ItemType = new ItemType() { MaximumBulk = 2f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "sturdyConstruction", //
                     //   DegradeType = "equipment",
                        SalvageProcess = "salvageCarpenterWorkshopUpgrade", 
                        PartKeys = new SerializableDictionary<string, int>()
                        {

                           { "item:sticks", 2 }, // for workbench. Not sure what materials should be used for the tabletop.
                             { "item:solidMudBrick", 2 },
                             { "item:barClamps", 1 },
                             { "item:shadeleafCanes", 1 } //for the lathe
                                    // table saw also? how is it powered?

                        },
                        Repair = "buildingRepair" 
                    },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "workshop" },
                        SpriteModifier = StateModifier.Upgrade5,
                        StorageSettings = "carpentersWorkshopStorage"
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ToolType = new ToolType()
                    {
                        Durability = ItemLoader.toolDurabilityUnbreakable,
                        ToolHandling = ToolHandlingType.Stationary                   
                    },

                    ContainerType = new ToolContainerType() // use this to hold output
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee firewood inside                  
                        ProductionOutputStorageType = new ItemStorageType(2f),
                        
                        // don't store fluids here, only wood and such
                        //StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)

                    },

                    RenderableType = new RenderableType() // should never be seen...
                    {
                        DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }
                    }
                });
                #endregion

                #region polymer workshop upgrade
                listOfEntityTypes.Add(new EntityType("item:polymerWorkshopUpgrade")
                {
                    Name = "Polymer workshop",
                    SummaryDescription = "Upgrade: For making plastic and rubber. Extrusion machine, vats and heating apparatus connected to a fireplace",
                    Description = "The extrusion machine works by pressing plastic or rubber feedstock through a heated die, thereby forming long, continuous products (tubes and sheets). When using a mould, the machine can also make smaller items.",//todo                    
                    ThumbnailSmall = "HUD_thumbnail_workshopPolymer",
                    ItemType = new ItemType() { MaximumBulk = 6f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "sturdyConstruction", //
                        //  DegradeType = "equipment",
                        SalvageProcess = "salvagePolymerWorkshopUpgrade", 
                        PartKeys = new SerializableDictionary<string, int>() 
                        { 
                            { "item:extrusionMachineComponents", 1 },
                           // { "item:wroughtIron", 3 }, 
                            { "item:solidMudBrick", 3 } // for fireplace
                        },
                        Repair = "buildingRepair" 
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" }, //used in headway tut
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "workshop" },
                        SpriteModifier = StateModifier.Upgrade6,
                        StorageSettings = "polymerWorkshopStorage"
                    },
                   
                    ToolType = new ToolType()
                    {
                        Durability = ItemLoader.toolDurabilityUnbreakable,
                        ToolHandling = ToolHandlingType.Stationary,
                        PrepareProcess = "lightFireFuelBurning" // this sets a special sprite modifier to trigger big smoke // "lightFire" //"kitchenImprovisedLightFire", //REQUIRED for the particle effects to appear. ...or use "lightFireWithoutFlames" or  "kilnSmoke"  ..mp maybe doesnt matter                   
                    },

                    ContainerType = new ToolContainerType() // NEW - use this to hold ouptut as well (nested container, hmm...)
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee firewood inside                  
                        ProductionOutputStorageType = new ItemStorageType(2f),
                        RequiresReplenishType = new RequiresReplenishType()
                        {
                            ReplenishProcess = "refuelCampfire",
                            RequiresFuelType = new RequiresFuelType()
                            {
                                MaxFuel = 1f,
                                FuelTypeTag = "fuelForCampfire",
                                BurnRatePerDay = 5f ////same as simple kitchen
                            }
                        },
                        StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }, //mp so fluids and liquids can be stockpiled here. (a sort of inbuilt clay jar + vat)

                    },                    
                   
                    RenderableType = new RenderableType() // should never be seen...
                    {
                        DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }                        
                    }                   
                });
                #endregion



                #region simple Stove Upgrade //mp is it used?
                //https://en.wikipedia.org/wiki/Kitchen_stove
                listOfEntityTypes.Add(new EntityType("item:simpleStoveUpgrade")
                {
                    Name = "Clay stove",
                    SummaryDescription = "Upgrade: Large stove and oven made from clay and stones",
                    Description = "This stove is moderately effective at cooking and baking big quantities of food.",
                    ItemType = new ItemType() { MaximumBulk = 5f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        //  DegradeType = "equipment",
                        DegradeType = "sturdyConstruction", //
                        SalvageProcess = "salvageSimpleStove",
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:solidMudBrick", 1},
                            { "item:stones", 1 }
                        },
                        Repair = "buildingRepair"
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Food },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "stove" },
                        SpriteModifier = StateModifier.UpgradeStove,
                     //   Effects = new string[] { "openStove" }
                        //Effects = bedEffects
                    },
                     ToolType = new ToolType()
                     {
                      //    ToolTag = new[]{  },
                          ToolHandling = ToolHandlingType.Stationary,
                          Durability = ItemLoader.toolDurabilityUnbreakable,
                          PrepareProcess = "lightFire" // "lightFire"   
                     },
                    ContainerType = new ReplenishContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee firewood inside                      
                        RequiresReplenishType = new RequiresReplenishType()
                        {
                            ReplenishProcess = "refuelCampfire",
                            RequiresFuelType = new RequiresFuelType()
                            {
                                MaxFuel = 1f,
                                FuelTypeTag = "fuelForCampfire",
                                BurnRatePerDay = 5f //     
                            }                           
                        }
                    },

                    RenderableType = new RenderableType() // should never be seen...copied MP
                    {
                        DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }
                    }    
                });
                #endregion

                #region Smoke Oven Upgrade
               
                listOfEntityTypes.Add(new EntityType("item:smokeOvenUpgrade")
                {
                    Name = "Smoke oven addition",
                    SummaryDescription = "Upgrade: Smoke oven built in addition to the cookhouse",
                    Description = "A convenient and efficient structure for smoking large quantities of fish and meat.",
                    ItemType = new ItemType() { MaximumBulk = 7f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        //  DegradeType = "equipment",
                        DegradeType = "sturdyConstruction", //
                        SalvageProcess = "salvageSmokeOvenUpgrade",//todo
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:solidMudBrick", 2},
                            { "item:stones", 1 }
                        },
                        Repair = "buildingRepair" 
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Food },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "smokeOven" },
                        SpriteModifier = StateModifier.UpgradeCookhouseSmokeOven,
                    },
                    ToolType = new ToolType()
                    {
                        //    ToolTag = new[]{  },
                        ToolHandling = ToolHandlingType.Stationary,
                        Durability = ItemLoader.toolDurabilityUnbreakable,
                        PrepareProcess = "lightFire" // "lightFire"   
                    },
                    ContainerType = new ReplenishContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee firewood inside                      
                        RequiresReplenishType = new RequiresReplenishType()
                        {
                            ReplenishProcess = "refuelSmokeOven", //todo
                            RequiresFuelType = new RequiresFuelType()
                            {
                                MaxFuel = 1f,
                                FuelTypeTag = "fuelForCampfire", ////todo
                                BurnRatePerDay = 4f //     
                            }
                        }
                    },

                    RenderableType = new RenderableType() // should never be seen...copied MP
                    {
                        DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }
                    }
                });
                #endregion

                #region Drying shed Upgrade

                listOfEntityTypes.Add(new EntityType("item:dryingShedUpgrade")
                {
                    Name = "Drying shed addition",
                    SummaryDescription = "Upgrade: Drying shed built in addition to the cookhouse",
                    Description = "A convenient and efficient structure for drying fish and meat.",
                    ItemType = new ItemType() { MaximumBulk = 6f },
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        //  DegradeType = "equipment",
                        DegradeType = "sturdyConstruction", 
                        SalvageProcess = "salvageDryingShedUpgrade",//
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:solidMudBrick", 1},
                            { "item:spoakShingles", 1 },
                             {"item:shadeleafCanes", 2 }
                        },
                        Repair = "buildingRepair" 
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Food },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "dryingShed" },
                        SpriteModifier = StateModifier.UpgradeCookhouseDryingShed,
                    },
                    ToolType = new ToolType()
                    {
                        //    ToolTag = new[]{  },
                        ToolHandling = ToolHandlingType.Stationary,
                        Durability = ItemLoader.toolDurabilityUnbreakable,                           
                    },
                    ContainerType = new ToolContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // is safe from scavengers

                        ProductionOutputStorageType = new ItemStorageType(4f) //
                    },

                    RenderableType = new RenderableType() // should never be seen...copied MP
                    {
                        DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }
                    }
                });
                #endregion


                #region community hall upgrade

            //todo comfort effect to colony. also storage.
                listOfEntityTypes.Add(new EntityType("item:communityHallUpgrade")
                {
                    Name = "Community hall",
                    SummaryDescription = "Upgrade: Community building addition to the cookhouse",
                    Description = "NOTE: the functionality of this building is work-in-progress!", //TODO!!!!!!!
                    ThumbnailSmall = "HUD_thumbnail_placeholder",
                    ItemType = new ItemType() { MaximumBulk = 40f },//very heavy building..
                    Category = GameData.Instance.AllEntityCategories["upgrades"],
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,                        
                        DegradeType = "sturdyConstruction", 
                        SalvageProcess = "salvageCommunityHall", 
                        PartKeys = new SerializableDictionary<string, int>()
                        {
                            { "item:solidMudBrick", 4 },
                            { "item:spoakBranchesTrimmed", 2 },
                            { "item:spoakShingles", 3 }
                        },
                        Repair = "buildingRepair" 
                    },
                    Upgrader = new Upgrader()
                    {
                        UpgradeCategories = new[] { "communityHall" },
                     //   Effects = bedEffects,
                        SpriteModifier = StateModifier.UpgradeCookhouseCommunityHall
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Comfort },
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }//todo
                });



                #endregion

            ///////////////////////////////////

                #region spoakLeaves
                listOfEntityTypes.Add(new EntityType("item:spoakLeaves")
                {
                    Name = "Spoak leaves",
                    SummaryDescription = "Leaves that are large, rigid plates",
                    Description = "The stiff leaves can offer protection against rain and wind when placed as overlapping tiles. But, after a time, they will attract the scuttler bug. It is therefore recommended to treat the plant material with a repellant if it is being used for long-term shelter.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.3f }, //AF 6/2-14 changed bulk from 0.5
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "proneToInfestation",
                        DegradesTo = "item:infestedLeavesRemains",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "spoakLeaves" } } } }
                });

                #endregion
                #region spoakLeavesRemains
                listOfEntityTypes.Add(new EntityType("item:infestedLeavesRemains")
                {
                    Name = "Infested leaf remains", //was: "Remains of spoak leaves" but made it general for other leaves
                    SummaryDescription = "The leaves have been eaten up by scuttler bugs and are no longer useful",
                    Description = "After a time, spoak leaves, wingweed leaves and daysheen will attract the scuttler bug which will eat the plant material. It is therefore recommended to treat the plant material with a repellant if it is being used for long-term shelter.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.2f }, 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "biowaste", //
                        DegradesTo = "item:organicMatter",

                    },
                    CategoryKey = "waste",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "biowaste" } } } }
                });

                #endregion
                #region spoakBranches


                listOfEntityTypes.Add(new EntityType("item:spoakBranches")
                {
                    Name = "Spoak branches",
                    SummaryDescription = "Hard, spiralling branches with large, stiff leaves",
                    Description = "These branches should be trimmed, because in this condition they are only useful for the crudest construction tasks.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.5f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "branches" } } }}
                });
                #endregion
                #region spoakBranchesTrimmed
                listOfEntityTypes.Add(new EntityType("item:spoakBranchesTrimmed")
                {
                    Name = "Spoak branches - trimmed",
                    SummaryDescription = "Spoak branches free of leaves and cut to size",
                    Description = "The hard, durable material is useful for building curving structures, but shaping the wood is difficult work.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.5f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "spoakWood" } } } }
                });

                #endregion
                #region spoakShingles
                listOfEntityTypes.Add(new EntityType("item:spoakShingles")
                {
                    Name = "Spoak shingles",
                    SummaryDescription = "Spoak leaf shingles for structures",
                    Description = "When made into shingles, the stiff leaves are an excellent cover for a shelter. The leaves are treated with a preservative which increases their resistance from bug infestation.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.2f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "spoakLeaves" } } }}
                });



                #endregion
                #region planks




                listOfEntityTypes.Add(new EntityType("item:planks")
                {
                    Name = "Planks",
                    ItemType = new ItemType() { MaximumBulk = 0.75f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.5f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "planks" } } }}
                });
                #endregion
                #region stones
                listOfEntityTypes.Add(new EntityType("item:stones")
                {
                    Name = "Stones",
                    Description = "Stones of various size, type and shape",
                    SummaryDescription = "Ordinary stones that could be used for simple construction",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.75f }, 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "stones" } } }}
                });
                #endregion
                #region soil
                listOfEntityTypes.Add(new EntityType("item:soil")
                {
                    Name = "Soil",
                    SummaryDescription = "An ordinary pile of soil with no special characteristics",
                    Description = "N/A",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.75f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "soil_s" } } }}
                });
                #endregion
                #region podlac
                listOfEntityTypes.Add(new EntityType("item:podlacUnrefined")
                {
                    Name = "Podlac (unrefined)",
                    Description = "Hard resin secreted by swamp insects",
                    SummaryDescription = "",//todo
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.75f }, 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "dirt"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "charcoal" } } }}
                });
                #endregion 

                #region turnipShell
                listOfEntityTypes.Add(new EntityType("item:turnipShell")
                {
                    Name = "Turnip shell",
                    SummaryDescription = "The shell of a turnip animal is exceptionally tough and thick",
                    Description = "The shell is sometimes used as a primitive hut. The location of the building is usually right where the animal died, owing to the great weight of the shell which makes it difficult to move.",
                    TierOrArea = new TierOrArea() { Tier = "survival" }, 
                    //  Icon = "", put in itembillboard 
                    ItemType = new ItemType() { HasNoMaximumBulk = true },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    SharedSpecialActions = new [] {new Pair<string, bool>( "constructTurnipHut", true ) },
                    ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always, // show the marker window with the options...
                    IsSelectable = true,
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "turnip",
                            ModelScale = 2.5f,
                            ModelBasicTextureName = "TurnipDarkTexture",
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                StartingPoint = Xclna.Xna.Animation.StartingPoint.Specified, // show the end frame of the hide anim as 'dead'                                
                                StartingPointInSeconds = 0.5f, // show the end frame of the hide anim as 'dead' - does not have to be accurate, will be clamped
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "hide" } }, // use "hide" in lieu of "dead"                                   
                            }
                        }
                    }
                });

                #endregion
                #region twinklerPlating
                listOfEntityTypes.Add(new EntityType("item:twinklerPlating")
                {
                    Name = "Quadite plating",
                    SummaryDescription = "Pieces of chitinous shell from quadites.",
                    Description = "N/A",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "chitinousshells" } } }}
                });
                #endregion
                #region improvisedGreenHouseCover
                listOfEntityTypes.Add(new EntityType("item:improvisedGreenHouseCover")
                {
                    Name = "Turnip gut sheet", //"Improvised greenhouse cover"
                    SummaryDescription = "Large, translucent sheet for use as a simple window",// maybe also use it as a hut upgrade?
                    Description = "Because of its translucency it can be used as a primitive substitute for transparent plastic, though not as durable. It shares some characteristics with the so-called goldbeater's skin, made from cow intestines, which was used on Earth for primitive balloons.",//
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                    ItemType = new ItemType() { MaximumBulk = 0.05f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tarpBeige" } } } }//placeholder asset??
                });
                #endregion

                #region megapodGreenHide
                listOfEntityTypes.Add(new EntityType("item:megapodGreenHide")
                {
                    Name = "Megapod fresh hide", 
                    SummaryDescription = "Recently removed hide of a megapod",
                    Description = "The hide needs to have flesh scraps and membranes removed before it spoils. A primitive way to preserve a hide is to treat it with the animal's own brain.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },//output from butchering
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:rottenMeat",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "hidePink" } } } }
                });
                #endregion
                #region megapodRawhide
                listOfEntityTypes.Add(new EntityType("item:megapodRawhide")
                {
                    Name = "Megapod rawhide",
                    SummaryDescription = "Animal skin. Stiff, but can bend to some extent",//
                    Description = "Can be made into simple hide products or it can be further processed (tanned) into a soft, tanned hide or leather.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "hideBeige" } } } }//
                });
                #endregion
                #region megapodTannedHide
                listOfEntityTypes.Add(new EntityType("item:megapodTannedHide")
                {
                    Name = "Megapod tanned hide",
                    SummaryDescription = "Soft, tanned hide which has increased durability",//
                    Description = "The hide has been softened by a tanning treatment.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tarpBeige" } } } }//
                });
                #endregion
                #region megapodBrain
                listOfEntityTypes.Add(new EntityType("item:megapodBrain") //MP wouldbe nice with just one animalBrain, but not supported to have same output from multiple processes
                {
                    Name = "Megapod brain", //http://animals.howstuffworks.com/mammals/deer-tan-own-hide1.htm
                    SummaryDescription = "A brain extracted from a megapod",//
                    Description = "The brain contains oils that can be used as a primitive tanning agent for hide treatment.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = 0.2f, ToolHandling = ToolHandlingType.HandTool }, //tool for tanning hides
                    ItemType = new ItemType() { MaximumBulk = 0.10f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:rottenMeat",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "brainsPink" } } } }
                });
                #endregion
                #region whipjawGreenHide
                listOfEntityTypes.Add(new EntityType("item:whipjawGreenHide")
                {
                    Name = "Whipjaw fresh hide", 
                    SummaryDescription = "Recently removed hide of a whipjaw",
                    Description = "The hide needs to have flesh scraps and membranes removed before it spoils. A primitive way to preserve a hide is to treat it with the animal's own brain.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },//mp there are both small and very large whipjaws
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:rottenMeat",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "hidePink" } } } }
                });
                #endregion
                #region whipjawRawhide
                listOfEntityTypes.Add(new EntityType("item:whipjawRawhide")
                {
                    Name = "Whipjaw rawhide",
                    SummaryDescription = "Animal skin. Stiff, but can bend to some extent",//
                    Description = "Can be made into simple hide products or it can be further processed (tanned) into a soft, tanned hide or leather.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "hideBeige" } } } }//
                });
                #endregion
                #region whipjawTannedHide
                listOfEntityTypes.Add(new EntityType("item:whipjawTannedHide")
                {
                    Name = "Whipjaw tanned hide",
                    SummaryDescription = "Soft, tanned hide which has increased durability",//
                    Description = "The hide has been softened by a tanning treatment.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tarpBeige" } } } }//
                });
                #endregion
                #region whipjawBrain
                listOfEntityTypes.Add(new EntityType("item:whipjawBrain") //MP wouldbe nice with just one animalBrain, but not supported to have same output from multiple processes
                {
                    Name = "Whipjaw brain", //http://animals.howstuffworks.com/mammals/deer-tan-own-hide1.htm
                    SummaryDescription = "A brain extracted from a whipjaw",//
                    Description = "The brain contains oils that can be used as a primitive tanning agent for hide treatment.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = 0.2f, ToolHandling = ToolHandlingType.HandTool }, //tool for tanning hides
                    ItemType = new ItemType() { MaximumBulk = 0.10f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:rottenMeat",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "brainsPink" } } } }
                });
                #endregion
                #region thunderChickenGreenHide
                listOfEntityTypes.Add(new EntityType("item:thunderChickenGreenHide")
                {
                    Name = "Thunder chicken fresh hide", //"Thunder chicken green hide"
                    SummaryDescription = "Recently removed hide of a thunder chicken",
                    Description = "The hide needs to have flesh scraps and membranes removed before it spoils. A primitive way to preserve a hide is to treat it with the animal's own brain.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:rottenMeat",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "hidePink" } } } }
                });
                #endregion
                #region thunderChickenRawhide
                listOfEntityTypes.Add(new EntityType("item:thunderChickenRawhide")
                {
                    Name = "Thunder chicken rawhide",
                    SummaryDescription = "Animal skin. Stiff, but can bend to some extent",//
                    Description = "Can be made into simple hide products or it can be further processed (tanned) into a soft, tanned hide or leather.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "hideBeige" } } } }//
                });
                #endregion
                #region thunderChickenTannedHide
                listOfEntityTypes.Add(new EntityType("item:thunderChickenTannedHide")
                {
                    Name = "Thunder chicken tanned hide",
                    SummaryDescription = "Soft, tanned hide which has increased durability",//
                    Description = "The hide has been softened by a tanning treatment.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tarpBeige" } } } }//
                });
                #endregion
                #region thunderChickenBrain 
                listOfEntityTypes.Add(new EntityType("item:thunderChickenBrain") //MP wouldbe nice with just one animalBrain, but not supported to have same output from multiple processes
                {
                    Name = "Thunder chicken brain", //http://animals.howstuffworks.com/mammals/deer-tan-own-hide1.htm
                    SummaryDescription = "The small brain has been extracted from the animal's back",//
                    Description = "The brain contains oils that can be used as a primitive tanning agent for hide treatment.",//
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = 0.2f, ToolHandling = ToolHandlingType.HandTool }, //tool for tanning hides
                    ItemType = new ItemType() { MaximumBulk = 0.05f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:rottenMeat",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "brainsPink" } } } }
                });
                #endregion
                #region thunderChickenBones
                listOfEntityTypes.Add(new EntityType("item:thunderChickenBones") //not currently output from butchering
                {
                    Name = "Thunder chicken bones",
                    ItemType = new ItemType() { MaximumBulk = 0.1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }}
                });
                #endregion
                #region bushDragonMeat
                listOfEntityTypes.Add(new EntityType("item:bushDragonMeat")
                {
                    Name = "Bush dragon meat",
                    SummaryDescription = "This meat is inedible by humans",//
                    Description = "N/A",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType() { MaximumBulk = 0.3f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:rottenMeat",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } }}
                });
                #endregion
                #region bushDragonPoisonGlands
                listOfEntityTypes.Add(new EntityType("item:bushDragonPoisonGlands")
                {
                    Name = "Bush dragon poison glands",
                    SummaryDescription = "The noxious fluid inside these glands could be of use to us",//
                    Description = "When care is taken, extracting the poison is a fairly simple matter.",
                    TierOrArea = new TierOrArea() { Tier = "survival" }, //output from butchering
                    ItemType = new ItemType() { MaximumBulk = 0.03f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:rottenMeat",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gutsyellow" } } }}
                });

                #endregion
                #region bushDragonWings


                listOfEntityTypes.Add(new EntityType("item:bushDragonWings")
                {
                    Name = "Bush dragon wings",
                    ItemType = new ItemType() { HasNoMaximumBulk = true },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:rottenMeat",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } }}
                });

                #endregion
                #region bushDragonBones
                listOfEntityTypes.Add(new EntityType("item:bushDragonBones")
                {
                    Name = "Bush dragon bones",
                    ItemType = new ItemType() { MaximumBulk = 0.3f },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "meatraw" } } }}
                });
                #endregion










                #region bow and arrow PARTS

                listOfEntityTypes.Add(new EntityType("item:improvisedBowLimb")
                {
                    Name = "Improvised bow limb",
                    SummaryDescription = "Made from a specially suited piece of wood",
                    Description = "Carefully shaped with a knife.",
                    ItemType = new ItemType() { MaximumBulk = 0.1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bowStave" } } } }
                });

                listOfEntityTypes.Add(new EntityType("item:improvisedArrowShaftBundle")
                {
                    Name = "Arrow shafts",
                    SummaryDescription = "Made from shadeleaf canes",
                    Description = "Carefully shaped with a knife and heat from a burning campfire.",
                    ItemType = new ItemType() { MaximumBulk = 0.04f }, //changed from 0.05f 22/5-14 AF
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "arrowShafts" } } }}
                });


           /*     listOfEntityTypes.Add(new EntityType("item:chitinousArrowhead")
                {
                    Name = "Chitinous arrowheads",
                    SummaryDescription = "Made from pieces of animal exoskeletons.",
                    Description = "Will somewhat increase penetration of the improvised arrows that we make.",
                    ItemType = new ItemType() { MaximumBulk = 0.02f }, //changed from 0.05 22/5-14 AF
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } } }}
                });*/

             /*   listOfEntityTypes.Add(new EntityType("item:improvisedMetalArrowHead")
                {
                    Name = "Metal arrowheads",
                    SummaryDescription = "Made from shards of metal",
                    Description = "Will greatly increase penetration of the improvised arrows that we make.",
                    ItemType = new ItemType() { MaximumBulk = 0.02f }, //changed from 0.05 22/5-14 AF
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } } }}
                });*/

             /*   listOfEntityTypes.Add(new EntityType("item:ironArrowHead")
                {
                    Name = "Arrowheads (iron)",
                    SummaryDescription = "Made from wrought iron",
                    Description = "Will greatly increase penetration of the handmade arrows that we make.",
                    ItemType = new ItemType() { MaximumBulk = 0.02f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry",
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } } } } //placeholder
                });*/
                #endregion

                #region xenoWipeAgent //not used
          /*      listOfEntityTypes.Add(new EntityType("item:xenoWipeAgent")
                {
                    Name = "XenoWipe agent",
                    ItemType = new ItemType() { MaximumBulk = 0.05f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bottle" } }}}
                });*/
                #endregion


 

                #region Spray weapon (Twinkler island)

                #region basicFireExtinguisher
                listOfEntityTypes.Add(new EntityType("item:basicFireExtinguisher")
                {
                    Name = "Fire extinguisher",  //its range can be improved by modifying the nozzle
                    SummaryDescription = "Uses cartridges. Has short range.",
                    Description = "Designed to control aircraft fires, this extinguisher has moderate discharge power to avoid dispersing burning material. \n A camp member suggests that we could improve its range and output if we had components such as a pump and various tubes.",
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.12f, //changed from 0.15 on 22/5-14 by AF


                        AttachedObjectRenderableType = "watergun", //MP: can I attach the watergunTank model as well?
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Watergun }

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                  //      PartKeys = new SerializableDictionary<string, int>() { { "item:fireExtinguisherTank", 1}, { "item:fireExtinguisherTube", 1}}
                    },
                   
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gadgets" } } } }                                  
                    
                });
                #endregion

                listOfEntityTypes.Add(new EntityType("item:fireSuppressantCartridge")
                {
                    Name = "Fire suppressant cartridge", //mp not sure if these should be in the game, too complex crafting.
                    SummaryDescription = "Ammunition for a fire extinguisher",
                    Description = "Used with a fire extinguisher to fight typical aircraft fires. Consists of a CO2 propellant and a fire suppressing compound. To combat other types of fires, the suppressant can be unloaded and exchanged with another kind of compound. \nA camp member has brought forward the idea to 'salvage' the cartridge, and filling it with a toxic compound to defend against quadites.",//change txt //this might be to scenario specific
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security }, //
                    ItemType = new ItemType() { MaximumBulk = 0.05f, }, // MP july 2014, changed from 0.2 because bushdragoncartridge has MaximumBulk = 0.05f
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageFireSuppressantCartridge",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:emptyCartridge", 1}}
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bottle" } } } }
                });

                listOfEntityTypes.Add(new EntityType("item:emptyCartridge")
                {
                    Name = "Empty cartridge",
                    SummaryDescription = "Empty cartridge for a fire extinguisher",
                    Description = "Consists of a CO2 propellant and an empty compound chamber. After being filled, the cartridge can be used in a fire extinguisher as 'ammunition'. \nSome camp members have suggested filling it with bush dragon poison to defend against quadites.", //a toxic compound to defend against quadites
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security }, //could be advanced. only relevant if used for trade in great descent
                    ItemType = new ItemType() { MaximumBulk = 0.03f, }, // MP july 2014, changed from 0.2 because bushdragoncartridge has MaximumBulk = 0.05f
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bottle" } } } }

                });

/*  not used
                listOfEntityTypes.Add(new EntityType("item:fireExtinguisherTank")
                {
                    Name = "Fire extinguisher tank",
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security }, //could be advanced. only relevant if used for trade in great descent
                    ItemType = new ItemType() { MaximumBulk = 0.06f }, //MP changed this from 0.2f. whole fire extinguisher has 0.12f
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "hydraulics" } }}}
                });

                listOfEntityTypes.Add(new EntityType("item:fireExtinguisherTube")
                {
                    Name = "Fire extinguisher tube",
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security }, //could be advanced. only relevant if used for trade in great descent
                    ItemType = new ItemType() { MaximumBulk = 0.04f }, //MP changed this from 0.1f. whole fire extinguisher has 0.12f
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "hydraulics" } }}}
                });
*/
/*          not used
                listOfEntityTypes.Add(new EntityType("item:fireExtinguisherNozzle")
                {
                    Name = "Improved nozzle for fire extinguisher",
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security }, //could be advanced. only relevant if used for trade in great descent
                    SummaryDescription = "Will make the fire extinguisher shoot longer distances",
                    Description = "An improvement that can make the fire extinguisher useful as a liquid weapon.",
                    ItemType = new ItemType() { MaximumBulk = 0.03f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "stored dry"
                    },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } }}}
                });
*/
//not used. we had a spray gun at one time, but was too complex to make:
            //        Name = "Liquid gun reservoir",
           //         SummaryDescription = "Liquid gun component. Holds the liquid",
            //        Description = "A component for a suggested chemical liquid gun. Consists of a tank, a cap for refilling, and an outlet to the pump area.",

               //     Name = "Liquid gun pressure chamber",
               //     SummaryDescription = "Liquid gun component. Stores pressure",
               //     Description = "A component for a suggested chemical liquid gun. The air pressure chamber is full of air when empty - as liquid is pumped in, the displaced air compresses.", //, generating pressure inversely proportional to the volume.

           //         Name = "Liquid gun pipes",
           //         SummaryDescription = "The frame for a chemical liquid gun",
           //         Description = "A component for a suggested chemical liquid gun. This part consists of the pump and the tubes that connect the pressure chamber with the liquid reservoir and leads liquid to the nozzle.",

                #endregion



                #region flintlockMechanism
                listOfEntityTypes.Add(new EntityType("item:flintlockMechanism")
                {
                    Name = "Flintlock mechanism",
                    SummaryDescription = "Ignition mechanism of 18th century design used for blackpowder firearms", //
                    Description = "A piece of flint held by a metal hammer. When triggered, the flint will hit a steel plate and make a spark which can ignite a blackpowder charge.",//
                    ItemType = new ItemType() { MaximumBulk = 0.04f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } } //placeholder asset
                });
                #endregion
                #region boltActionMechanism
                listOfEntityTypes.Add(new EntityType("item:boltActionMechanism")
                {
                    Name = "Bolt-action mechanism",
                    SummaryDescription = "Simple firearm mechanism for firing metal cartridges", //
                    Description = "Components that allow the spent cartridge case to be ejected and a new one to be placed in the breech.",//
                    ItemType = new ItemType() { MaximumBulk = 0.04f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Security },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } } //placeholder asset
                });
                #endregion
                #region landMine
                listOfEntityTypes.Add(new EntityType("item:landMine")
                {
                    Name = "Land mine",
                    SummaryDescription = "Explosive device triggered by pressure", //
                    Description = "Simple land mine made from blackpowder and a flintlock trigger mechanism. Must be deployed as a structure.",//
                    ItemType = new ItemType() { MaximumBulk = 0.12f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wire" } } } } //placeholder asset
                });
                #endregion

                #region unfiredClayJar
                listOfEntityTypes.Add(new EntityType("item:unfiredClayJar")
                {
                    Name = "Clay jar (unfired)", //was "Unfired clay jar"
                    SummaryDescription = "Half finished clay jar",
                    Description = "This piece of pottery is dry but needs to be fired and glazed in a kiln before it can be of use.",
                    TierOrArea = new TierOrArea() { Tier = "basic" }, //like clay
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "clayPotBeige" } } } }
                });
                #endregion
                #region unfiredClayPot
                listOfEntityTypes.Add(new EntityType("item:unfiredClayPot")
                {
                    Name = "Clay pot (unfired)", //was: "Unfired clay pot"
                    SummaryDescription = "Half finished clay pot",
                    Description = "This piece of pottery is dry but needs to be fired in a kiln before it can be of use.",
                    TierOrArea = new TierOrArea() { Tier = "basic" }, //like clay
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "clayPotSmallBeige" } } } }
                });
                #endregion

                #region unfiredBulletMold
                listOfEntityTypes.Add(new EntityType("item:unfiredBulletMold")
                {
                    Name = "Bullet mold (unfired)",
                    SummaryDescription = "Half finished bullet mold",
                    Description = "This piece of ceramics is dry but needs to be fired in a kiln before it can be of use.",
                    TierOrArea = new TierOrArea() { Tier = "basic" }, //like clay
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "brickMold" } } } }
                });
                #endregion
                #region liquidGas
                listOfEntityTypes.Add(new EntityType("item:liquidGas")
                {
                    Name = "Liquid gas",
                    SummaryDescription = "Flammable gas for heating, cooking or running certain engines.",
                    Description = "The gas is pressurized and in liquid form.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType() { MaximumBulk = 0.5f, FuelType = new FuelType() { FuelTags = new[] { "fuelForFieldKitchen" } } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "drum" } } }}
                });
  
 
                #endregion

                #endregion

                #region WEAPONS

                #region coilRifle
                listOfEntityTypes.Add(new EntityType("item:coilRifle")
                {
                    Name = "Coil rifle", // http://en.wikipedia.org/wiki/Coilgun
                    SummaryDescription = "Electromagnetic gun.",
                    Description = "Part of the standard issue equipment for the Tau Ceti Program. Range, precision and power is efficient against all types of native wildlife. The gun works by accelerating the (ferromagnetic) projectile through a series of electromagnetic coils.",
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.08f, //changed weight from 0.1 to 0.08 on 22/5-14 by AF
                        
                        AttachedObjectRenderableType = "rifle",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Rifle },
                        WeaponType = new WeaponType()
                        {                           
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["shootCoilRifle"] },
                           
                        },
                        
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside?
                       
                        MaxCapacity = 15, //mp always half of clip size, so they divide the clip and carry extra
                        UsesAmmoTypeKeyName = "item:coilRifleAmmo",
                        ReplenishProcess = "reloadCoilRifleAmmo"                          
                        
                    },
                    CategoryKey = "weapons",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } }}
                });
                #endregion
                #region boltActionRifle 
                listOfEntityTypes.Add(new EntityType("item:boltActionRifle")
                {
                    Name = "Bolt-action rifle",
                    SummaryDescription = "For hunting and defense.", // http://en.wikipedia.org/wiki/Bolt_action
                    Description = "A rugged rifle which is loaded with metal cartridge ammunition by using a manually operated bolt mechanism.",
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f, 

                        AttachedObjectRenderableType = "rifle",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Rifle },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["shootCorditeRifledBullet"] }                          
                        },

                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageBoltActionRifle",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:gunBarrelRifled", 1 }, { "item:gunStock", 1 }, { "item:boltActionMechanism", 1 } } 
                    },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside?

                        MaxCapacity = 10, //mp always half of clip size, so they divide the clip and carry extra
                        UsesAmmoTypeKeyName = "item:corditeAmmo", 
                        ReplenishProcess = "reloadBoltActionRifle"                          
                        
                    },
                    CategoryKey = "weapons",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } } }                    
                });
                #endregion
                #region gunpowderRifle
                listOfEntityTypes.Add(new EntityType("item:gunpowderRifle") //
                {
                    Name = "Black powder rifle",
                    SummaryDescription = "Breech-loading flintlock rifle.", //
                    Description = "Has high precision and rate of fire when taken into account its low tech manufacturing process. The design is somewhat similar to a Ferguson rifle of the 18th century: The weapon is loaded from the breech with black powder and a rifle bullet. Must be reloaded often.", 
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.14f,
                        AttachedObjectRenderableType = "rifle",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Rifle },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["shootGunpowderRifle"] }
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageGunpowderRifle",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:gunBarrelRifled", 1 }, { "item:gunStock", 1 }, { "item:flintlockMechanism", 1 } } 
                    },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside
                        MaxCapacity = 3, // //mp always half of clip size, so they divide the clip and carry extra
                        UsesAmmoTypeKeyName = "item:blackPowderRifleAmmo",
                        ReplenishProcess = "reloadGunpowderRifle"     //todo check these values
                    },
                    CategoryKey = "weapons",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } } }
                    
                });
                #endregion
                #region musket
                listOfEntityTypes.Add(new EntityType("item:musket") //
                {
                    Name = "Musket", //
                    SummaryDescription = "Muzzleloading, flintlock smooth-bore.", //
                    Description = "A simple firearm that fires smooth gold balls using black powder. Has limited range and accuracy and must be reloaded often.", //, single-shot weapon
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.14f, //heavyweapon?
                        AttachedObjectRenderableType = "rifle",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Rifle },
                        WeaponType = new WeaponType()
                        {                                                                      
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["shootUnrifledBullet"], GameData.Instance.AllAttackTypes["hitWithRifleButt"] }
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Best }, //maybe these shouldn't be marked as best??
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageMusket",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:gunBarrelSmoothLong", 1 }, { "item:gunStock", 1 }, { "item:flintlockMechanism", 1 } }
                    },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" },

                        MaxCapacity = 3, //mp always half of clip size, so they divide the clip and carry extra
                        UsesAmmoTypeKeyName = "item:blackPowderShotAmmo",
                        ReplenishProcess = "reloadBlunderbuss"
                    },
                    CategoryKey = "weapons",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } } }                    
                });
                #endregion
                #region musketoon
                listOfEntityTypes.Add(new EntityType("item:musketoon") //
                {
                    Name = "Musketoon", //was: "Blunderbuss gun"  but changed because it does not have a MUCH bigger caliber than the musket, simply shorter and a bit bigger bore  (comes from same material.)
                    SummaryDescription = "Muzzleloading flintlock shotgun.", //
                    Description = "A simple firearm that fires a number of small projectiles in a single blast. Basically a shorter musket with bigger bore which increases the spread, giving better accuracy but shorter range. Useful for vermin hunting. Must be reloaded often.", //
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f, //shorter
                        AttachedObjectRenderableType = "rifle",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Rifle },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["shootBlunderbuss"], GameData.Instance.AllAttackTypes["hitWithRifleButt"] }                           
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Best }, //maybe these shouldn't be marked as best??
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageMusketoon",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:gunBarrelSmoothShort", 1 }, { "item:gunStock", 1 }, { "item:flintlockMechanism", 1 } } 
                    },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" },

                        MaxCapacity = 3, ////mp always half of clip size, so they divide the clip and carry extra 
                        UsesAmmoTypeKeyName = "item:blackPowderShotAmmo",
                        ReplenishProcess = "reloadBlunderbuss"                        
                    },
                    CategoryKey = "weapons",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } } },
                    
                });
                #endregion
                #region shotgun
                listOfEntityTypes.Add(new EntityType("item:shotgun") //
                {
                    Name = "Pump-action shotgun",
                    SummaryDescription = "Rugged and very effective shotgun.", //
                    Description = "Designed for the Tau Ceti mission with the intention of having easily replaceable parts. A smoothbore firearm which uses cordite-propelled cartridges to fire a number of pellets in each blast. The handgrip is pumped between each shot to replace the cartridge in the chamber.", //
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.08f,
                        AttachedObjectRenderableType = "rifle",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Rifle },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["shootShotgun"], GameData.Instance.AllAttackTypes["hitWithRifleButt"] }
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Best }, //maybe these shouldn't be marked as best??
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageShotgun",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:gunBarrelSmoothShort", 1 }, { "item:gunStock", 1 } }  
                    },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" },
                        MaxCapacity = 10, //mp always half of clip size, so they divide the clip and carry extra 
                        UsesAmmoTypeKeyName = "item:shotgunAmmo",
                        ReplenishProcess = "reloadShotgun"                           
                        
                    },
                    CategoryKey = "weapons",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } } }                    
                });
                //
                #endregion


                #region Spray weapon - improvedFireExtinguisher
                listOfEntityTypes.Add(new EntityType("item:improvedFireExtinguisher")
                {
                    Name = "Spray weapon",
                    SummaryDescription = "Improvised weapon. Can spray a powerful discharge of toxic liquid",
                    Description = "This modified fire extinguisher allows for powerful discharges of a toxic liquid over long range and will be effective against twinklers.",
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.13f, 
                                                 

                        AttachedObjectRenderableType = "watergun", //MP: can I attach the watergunTank model as well?
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Watergun },

                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                         {
                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Best },
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best }, //mp apr 2015 i put this in. it was not used
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None }, // don't use this when hunting unspecified prey. MP feb 25 2015: but they seem to use it for hunting specified prey such as other animals than twinklers.
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None }, //mp I'm setting this as minor/none for all...
                         },
                        WeaponType = new WeaponType()
                        {                           
                            AttackTypes = new AttackType[] {                                
                                GameData.Instance.AllAttackTypes["shootImprovedFireExtinguisherBushDragonPoison"]}

                        }

                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                 //       PartKeys = new SerializableDictionary<string, int>() { { "item:fireExtinguisherTank", 1}, { "item:fireExtinguisherTube", 1}}          //  
                    },

                    ContainerType = new MagazineContainerType()
                {
                    CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside

                    MaxCapacity = 10, //mp always half of clip size, so they divide the clip and carry extra 
                    //AmmoTypeKeyName = "item:coilRifleAmmo",
                    UsesAmmoTag = "fireExtinguisherAmmo",
                    ReplenishProcess = "reloadFireExtinguisher"                      
                    
                },

                    CategoryKey = "weapons",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gadgets" } } }}                             
                    
                });
                #endregion


              
                #region bushDragonPoison //not used. we have fire extinguisher and "fireExtinguisherAmmo" only
        /*        listOfEntityTypes.Add(new EntityType("item:bushDragonPoison") 
                {
                    Name = "Bush dragon liquid ammo",
                    SummaryDescription = "A toxic liquid to be used as ammunition for a liquid gun.",
                    Description = "", // It can also be combined with a fire extinguisher cartridge to be used as ammo for the fire extinguisher.
                });*/
                #endregion


                #region sentry item
                listOfEntityTypes.Add(new EntityType("item:sentry")
                {
                    Name = "Sentry (item)",
                SummaryDescription = "Autonomous gun turret for area defence",
                Description = "The TRIAAD is a stationary machine gun for area defence. It has sensors and a degree of AI for operating in all conditions, and has been optimized for an alien environment containing unknown threats. Its low power consumption and deep magazine makes it able to operate unsupervised for extended periods of time.",
                    ItemType = new ItemType() { MaximumBulk = 0.83f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = true,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageSentryItem",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:sentryGun", 1 }, { "item:sentryWeaponMount", 1 } }
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },//advanced  because this is an un-improvised item.   note that the sentry structure is survival (once they have it they might as well put it up)
                    CategoryKey = "weapons",                    
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } }}
                });
                #endregion

                #region sprayGunSentry item
                listOfEntityTypes.Add(new EntityType("item:spraySentry")
                {
                    Name = "Spray gun sentry (item)",
                SummaryDescription = "Autonomous turret, modified with an improvised spray gun",
                    Description = "We have dismounted the machine gun and jerry-rigged a fire extinguisher gun onto the turret, making it able to, hopefully, shoot a poisonous liquid at approaching twinklers.",
                    ItemType = new ItemType() { MaximumBulk = 0.83f, },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },//'survival' because it has been improvised, put together from parts
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = true,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageSpraySentryItem",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:sentrySprayGun", 1 }, { "item:sentryWeaponMount", 1 } }
                    },
                    CategoryKey = "weapons",                    
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } } }
                });
                #endregion

                #region shotgunSentry item
                listOfEntityTypes.Add(new EntityType("item:shotgunSentry")
                {
                    Name = "Shotgun sentry (item)",
                    SummaryDescription = "Autonomous turret outfitted with a shotgun. Made with improvised methods.",
                    Description = "We have dismounted the machine gun and attached a shotgun onto the turret.",
                    ItemType = new ItemType() { MaximumBulk = 0.83f, },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },//'survival' because it has been improvised, put together from parts
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = true,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageShotgunSentryItem",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:sentryShotgun", 1 }, { "item:sentryWeaponMount", 1 } },
                    },
                    CategoryKey = "weapons",                    
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } } }
                });
                #endregion

                #region improvisedBow
                listOfEntityTypes.Add(new EntityType("item:improvisedBow")
                {
                    Name = "Bow (improvised)",
                    SummaryDescription = "Simple bow made from wood. Ammunition: Arrows",
                    Description = "This bow offers some advantage over hand weapons, particularly when sneaking up on faster prey.",
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.07f, 

                        AttachedObjectRenderableType = "bow",
                        AttachorTagToMountOn = "leftHand",
                        AttachesToBodyPart = "Left arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Bow },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { 
                                GameData.Instance.AllAttackTypes["shootImprovisedBasicArrow"],
                                GameData.Instance.AllAttackTypes["shootImprovisedChitinousArrow"],
                                GameData.Instance.AllAttackTypes["shootImprovisedMetalArrow"],
                                GameData.Instance.AllAttackTypes["shootIronArrow"],}
                        },                       
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Best }, //mp: if they have bows, player likely want them to be used for hunting just as much (if not more) than the rifle (to conserve ammo)
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal } ,
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }

                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "improvisedEquipment",
                    },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside
                        MaxCapacity = 5, //mp always half of clip size, so they divide the clip and carry extra 
                        UsesAmmoTag = "arrow",
                        ReplenishProcess = "reloadImprovisedBow"                  
                    },
                    CategoryKey = "weapons",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bow" } } } }
                });
                #endregion

                #region improvised crude spear
                listOfEntityTypes.Add(new EntityType("item:improvisedBasicSpear")
                {
                    Name = "Spear (crude)",
                    SummaryDescription = "Long, sharpened pole. Serves as both tool and weapon.",
                    Description = "A crude weapon that doubles as a tool for some fishing and gathering of small prey.",
                    Icon = "spear",
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,

                    },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f, //changed 22/5-14 from 0.15 by AF

                        AttachedObjectRenderableType = "spear",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Spear },
                        WeaponType = new WeaponType()
                        {
                            //     ReloadTime = 3f, //Thrown spear
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personCrudeSpearThrustMid"] },  //Thrown spear

                        }

                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },

                    CategoryKey = "weapons",//was "tools" but changed sep 2016

                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType() { AssetName = "spear", ModelScale = 2f }
                    }, // RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "spearMachete" } } },
                    //     LocomotorType = new LocomotorType() { BallisticLocomotorType = new BallisticLocomotorType() { } },   //Thrown spear

                });
                #endregion
                #region ironSpear
                listOfEntityTypes.Add(new EntityType("item:ironSpear")
                {
                    Name = "Spear (iron-tipped)",
                    SummaryDescription = "An handmade spear outfitted with a sharp spearhead made from wrought iron. Serves as both tool and weapon.",
                    Description = "A reliable weapon that doubles as a tool for some fishing and gathering of small prey.",
                    Icon = "spear",
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,

                    },
                    LocomotorType = new LocomotorType()
                    {
                        BallisticLocomotorType = new BallisticLocomotorType() { },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            BallisticResponderType = new BallisticResponderType() { }
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f,
                        AttachedObjectRenderableType = "spear",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Spear },

                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal }, //
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, // 
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor}, //mp I'm setting this as minor/none for all...
                         },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personMetalSpearThrustMid"] }
                        }

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        // PartKeys = new SerializableDictionary<string, int>() { { "item:ironSpearhead", 1 } }
                    },
                    CategoryKey = "weapons",//was "tools" but changed sep 2016
                    RenderableType = new RenderableType() { RenderAsModelType = new RenderAsModelType() { AssetName = "spear", ModelScale = 2f } }
                });

                #endregion
                #region improvisedGoodSpear Metal-tipped
                listOfEntityTypes.Add(new EntityType("item:improvisedGoodSpear")
                {
                    Name = "Spear (scrap metal)",
                    SummaryDescription = "An improvised spear outfitted with a sharp metal head. Serves as both a tool and a weapon.",
                    Description = "A simple weapon that doubles as a tool for some fishing and gathering of small prey.",
                    Icon = "spear",
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,

                    },
                    LocomotorType = new LocomotorType()
                    {
                        BallisticLocomotorType = new BallisticLocomotorType() { },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            BallisticResponderType = new BallisticResponderType() { }
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f, //changed 22/5-14 by AF from 0.15

                        AttachedObjectRenderableType = "spear",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Spear },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal }, //
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, // 
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor}, //mp I'm setting this as minor/none for all...
                         },

                        WeaponType = new WeaponType()
                        {

                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personMetalSpearThrustMid"] }  //Thrown spear:  DON't use!!! is a bug generator and ugly too!! -MP  ,  GameData.Instance.AllAttackTypes["throwSpear"]

                        }

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        // PartKeys = new SerializableDictionary<string, int>() { { "item:improvisedMetalSpearhead", 1} }
                    },
                    CategoryKey = "weapons",//was "tools" but changed sep 2016
                    //RenderableType = new RenderableType() { Default = new StaticConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "spearMachete" } } },

                    RenderableType = new RenderableType() { RenderAsModelType = new RenderAsModelType() { AssetName = "spear", ModelScale = 2f } },
                    //     LocomotorType = new LocomotorType() { BallisticLocomotorType = new BallisticLocomotorType() { } },   //Thrown spear

                });
                #endregion

                #region knife spear -made from a knife.
                listOfEntityTypes.Add(new EntityType("item:advancedKnifeSpear")
                {
                    Name = "Knife spear (improvised)",
                    SummaryDescription = "A knife attached to a long shaft (Retrieve the knife by salvaging the spear). Serves as both tool and weapon",
                    Description = "This improvised weapon is an efficient way to extend the knife's reach for use in hunting, defense and fishing.",
                    Icon = "spear",
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,

                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    LocomotorType = new LocomotorType()
                    {
                        BallisticLocomotorType = new BallisticLocomotorType() { },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            BallisticResponderType = new BallisticResponderType() { }
                        }
                    },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f, //

                        AttachedObjectRenderableType = "spear",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Spear },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal }, //
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, // 
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor}, //mp I'm setting this as minor/none for all...
                         },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personMetalSpearThrustMid"] }
                        }

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageKnifeSpear", //retrieving the knife
                        PartKeys = new SerializableDictionary<string, int>() { { "item:advancedKnife", 1 } }

                    },
                    CategoryKey = "weapons",//was "tools" but changed sep 2016
                    //RenderableType = new RenderableType() { Default = new StaticConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "spearMachete" } } },

                    RenderableType = new RenderableType() { RenderAsModelType = new RenderAsModelType() { AssetName = "spear", ModelScale = 2f } }
                });
                #endregion

                #region improvisedFlintSpear
                listOfEntityTypes.Add(new EntityType("item:improvisedFlintSpear")
                {
                    Name = "Spear (flint-tipped)",
                    SummaryDescription = "An improvised spear tipped with a sharp piece of flint. Serves as both tool and weapon",
                    Description = "A simple weapon that doubles as a tool for some fishing and gathering of small prey.",
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    Icon = "spear",
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,

                    },
                    LocomotorType = new LocomotorType()
                    {
                        BallisticLocomotorType = new BallisticLocomotorType() { },
                        CollisionResponderType = new CollisionResponderType()
                        {
                            BallisticResponderType = new BallisticResponderType() { }
                        }
                    },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f, //changed 22/5-14 by AF from 0.15

                        AttachedObjectRenderableType = "spear",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Spear },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal }, //
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, // 
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor}, //mp I'm setting this as minor/none for all...
                         },
                        WeaponType = new WeaponType()
                        {
                            //     ReloadTime = 3f, //Thrown spear
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personFlintSpearThrustMid"] },  //Thrown spear:  DON't use!!! is a bug generator and ugly too!! -MP  ,  GameData.Instance.AllAttackTypes["throwSpear"]

                        }

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        // DegradesTo = "item:flintRough"
                        //  PartKeys = new SerializableDictionary<string, int>() {  { "item:flintSpearhead", 1}}   
                    },
                    CategoryKey = "weapons",//was "tools" but changed sep 2016

                    RenderableType = new RenderableType() { RenderAsModelType = new RenderAsModelType() { AssetName = "spear", ModelScale = 2f } },
                    //     LocomotorType = new LocomotorType() { BallisticLocomotorType = new BallisticLocomotorType() { } },   //Thrown spear

                });

                #endregion
   
                #endregion
                #region AMMUNITION
                #region bushDragonCartridge
                listOfEntityTypes.Add(new EntityType("item:bushDragonCartridge")
                {
                    Name = "Bush dragon cartridge",
                    SummaryDescription = "A fire extinguisher cartridge with bush dragon poison.",
                    Description = "The cartridge can be used as ammo for a spray weapon, a modified fire extinguisher. Contains a toxic bush dragon agent and a CO2-propellant.",
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    ItemType = new ItemType() { MaximumBulk = 0.05f, AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 20, AmmoTags = new[] { "fireExtinguisherAmmo" } }, HauledItemValue = ItemType.HauledItemValues.MostValuable, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bottle" } } } }
                });
                #endregion
                #region coilRifleAmmo
                listOfEntityTypes.Add(new EntityType("item:coilRifleAmmo")
                {
                    Name = "Coil rifle ammo clip",
                    SummaryDescription = "Ammunition for a coil gun",
                    Description = "Solid projectiles made from a ferromagnetic steel-alloy",
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.03f,



                        AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 30 }
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" /*"guns"*/ } } } }
                });
                #endregion
                #region sentryGunAmmo
                listOfEntityTypes.Add(new EntityType("item:sentryGunAmmo")
                {
                    Name = "Sentry gun ammo",
                    SummaryDescription = "Ammunition for the sentry robot",
                    Description = "Cartridges, each containing chemical propellant and a bullet",
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.06f,
                        AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 100 }
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" /*"guns"*/ } } } }
                });
                #endregion
                #region corditeAmmo,  Bolt-action rifle ammo
                listOfEntityTypes.Add(new EntityType("item:corditeAmmo")
                {
                    Name = "Bolt-action rifle ammo",
                    SummaryDescription = "Ammunition clip for a bolt-action rifle",
                    Description = "Several cartridges with cordite propelled projectiles.", //change txt?
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.03f,

                        AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 20 }
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" /*"guns"*/ } } } }
                });
                #endregion
                #region BlackPowderRifleAmmo
                listOfEntityTypes.Add(new EntityType("item:blackPowderRifleAmmo")
                {
                    Name = "Black powder rifle ammo",
                    SummaryDescription = "Ammunition for a black powder rifle", //
                    Description = "A pouch with black powder and a small bag with rifled gold bullets. The high density and ductility of gold makes for an advantageous projectile material.", //
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.05f,
                        AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 6 } //  todo    note that crafting takes time, adjust production output. 
                    },
                    TierOrArea = new TierOrArea() { Tier = "medium", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "improvisedEquipment",
                        SalvageProcess = "salvageBlackPowderRifleAmmo",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:goldBullet", 1 }, { "item:blackPowder", 1 }}
                    },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } } //placeholder asset    
                    
                });
                #endregion
                #region shotgunAmmo
                listOfEntityTypes.Add(new EntityType("item:shotgunAmmo")
                {
                    Name = "Shotgun shells",
                    SummaryDescription = "Ammunition cartridges for a shotgun",
                    Description = "Plastic cases containing primer, powder charge and tungsten pellets",// https://en.wikipedia.org/wiki/Shotgun_shell 
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.03f,
                        AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 20 }
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } }//placeholder asset
                });
                #endregion
                #region BlackPowderShotAmmo
                listOfEntityTypes.Add(new EntityType("item:blackPowderShotAmmo")
                {
                    Name = "Musket/musketoon ammo",
                    SummaryDescription = "Ammunition for a black powder smooth bore firearm such as a musket or musketoon",//
                    Description = "A pouch with black powder and a small bag with gold balls to be used in a non-rifled firearm. The high density and ductility of gold makes for an advantageous projectile material.",//
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f,
                        AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 6 }
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "improvisedEquipment",
                        SalvageProcess = "salvageBlackPowderShotAmmo",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:blunderbussBalls", 1 }, { "item:blackPowder", 1 }, }
                    },
                    CategoryKey = "ammunition",                    
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } }//placeholder asset
                });
                #endregion

                #region improvisedBasicArrow
                listOfEntityTypes.Add(new EntityType("item:improvisedBasicArrow")
                {
                    Name = "Arrows (crude)",
                    SummaryDescription = "Bunch of simple arrows without arrowheads or fletchings",
                    Description = "These arrows, although basically sharpened sticks, still offer a chance to hunt prey that cannot be caught by hand.",
                    ItemType = new ItemType() { MaximumBulk = 0.04f, AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 10, AmmoTags = new[] { "arrow" } }, HauledItemValue = ItemType.HauledItemValues.MostValuable, }, //changed bulk from 0.02 22/5-14 AF
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "improvisedEquipment",
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "arrowShafts" } } } }
                });
                #endregion
                #region improvisedChitinousArrow
                listOfEntityTypes.Add(new EntityType("item:improvisedChitinousArrow")
                {
                    Name = "Arrows (chitin)",
                    SummaryDescription = "Bunch of arrows fitted with sharp chitin heads and fletchings",
                    Description = "We can use animal materials to make a more deadly arrow.",
                    ItemType = new ItemType() { MaximumBulk = 0.05f, AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 10, AmmoTags = new[] { "arrow" } }, HauledItemValue = ItemType.HauledItemValues.MostValuable, }, //changed bulk from 0.02 22/5-14 AF
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "improvisedEquipment",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:improvisedArrowShaftBundle", 1 } } // NA had { "item:chitinousArrowhead", 1}
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "arrowShafts" } } } } 

                });
                #endregion
                #region improvisedMetalArrow
                listOfEntityTypes.Add(new EntityType("item:improvisedMetalArrow")
                {
                    Name = "Arrows (scrap metal)",
                    SummaryDescription = "Bunch of arrows with fletchings and sharp heads made of metal",
                    Description = "Pieces of scrap metal made into arrowheads and used on arrows.",
                    ItemType = new ItemType() { MaximumBulk = 0.05f, AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 10, AmmoTags = new[] { "arrow" } }, HauledItemValue = ItemType.HauledItemValues.MostValuable, }, //changed bulk from 0.02 22/5-14 AF
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "improvisedEquipment",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:improvisedArrowShaftBundle", 1 } }  // NA   removed  { "item:improvisedMetalArrowHead", 1}
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Security },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "arrowShafts" } } } }
                               
                });
                #endregion
                #region ironArrow
                listOfEntityTypes.Add(new EntityType("item:ironArrow")
                {
                    Name = "Arrows (iron)",
                    SummaryDescription = "Bunch of arrows with fletchings and sharp heads made of iron",
                    Description = "Reliable arrows outfitted with sharp arrow heads made from wrought iron", //
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.05f,
                        AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 10, AmmoTags = new[] { "arrow" } },
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "improvisedEquipment",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:improvisedArrowShaftBundle", 1 } } // NA Removed , { "item:ironArrowHead", 1 }, 
                    },
                    CategoryKey = "ammunition",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "arrowShafts" } } } }                    
                });
                #endregion
                #endregion
                #region BODIES

                listOfEntityTypes.Add(new EntityType("item:turnipCarcass")
                {
                    Name = "Turnip carcass",
                    SummaryDescription = "Carcass from the Turnip creature",
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.35f },
                              {"shell", 0.35f },
                              {"guts", 0.25f },
                              {"hide", 0.05f }
                          }
                    }, 
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType() { AssetName = "turnip", ModelScale = 2.5f, ModelBasicTextureName = "TurnipDarkTexture",
                            // this will show the correct frame instead of just the bind pose:
                                DefaultInfo = new AnimConditionInfo() 
                                { 
                                    Looping = Xclna.Xna.Animation.Looping.No,
                                    StartingPoint = Xclna.Xna.Animation.StartingPoint.Specified, // show the end frame of the hide anim as 'dead'                                
                                    StartingPointInSeconds = 0.5f, // show the end frame of the hide anim as 'dead' - does not have to be accurate, will be clamped
                                    SpeedFactor = 0f, // freeze the anim
                                    SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "hide" } }, // use "hide" in lieu of "dead"                                   
                                }                            
                        }
                    }
                });
                listOfEntityTypes.Add(new EntityType("item:body")
                {
                    Name = "Human body",
                    SummaryDescription = "A dead person's remains",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    RenderableType = new RenderableType()
                {
                    RenderAsModelType = new RenderAsModelType() { AssetName = "man",
                                                                  ModelScale = 2f, // these default values are used when the body was never 'living' and cannot inherit textures and scales from that entity
                                                                  ModelBasicTextureName = "ManGrey1Texture",                                     
                        // this will show the correct frame instead of just the bind pose:
                                                                  DefaultInfo = new AnimConditionInfo()
                                                                  {
                                                                      Looping = Xclna.Xna.Animation.Looping.No,
                                                                      SpeedFactor = 0f, // freeze the anim
                                                                      SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                                                                  }
                    }
                }
                });
                listOfEntityTypes.Add(new EntityType("item:quaditeCarcass")
                {
                    Name = "Quadite carcass",
                    SummaryDescription = "A dead quadite",
                    Icon = "HUD_icon_carcass",
                    Description = "The quadite carcass is quickly reduced to a husk, as its own parasites feed on their dead host.", 
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:twinklerPlating",  //degrades to this because the scamp grubs quickly devour its flesh. then they turn into scamp beetle and fly away. if it's butchered before, the meat  will turn into a scamp grub
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.3f },                              
                              {"guts", 0.2f },
                              {"plating", 0.5f }
                          }
                    },
                    RenderableType = new RenderableType()
                {
                    RenderAsModelType = new RenderAsModelType()
                    {
                        AssetName = "twinkler",
                        ModelScale = 2.5f, 
                        ModelBasicTextureName = "QuaditeRedTexture", // perhaps create special carcass items for other textures...                                         
                        // this will show the correct frame instead of just the bind pose:
                        DefaultInfo = new AnimConditionInfo()
                        {
                            Looping = Xclna.Xna.Animation.Looping.No,
                            SpeedFactor = 0f, // freeze the anim
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } }, 
                        }
                    }
                }
                });
                listOfEntityTypes.Add(new EntityType("item:swarmerCarcass")
                {
                    Name = "Swarmer carcass",
                    SummaryDescription = "A dead swarmer",
                    Icon = "HUD_icon_carcass",
                    Description = "The swarmer carcass is quickly reduced to a husk, as its own parasites feed on their dead host.",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:twinklerPlating",  //degrades to this because the scamp grubs quickly devour its flesh. then they turn into scamp beetle and fly away. if it's butchered before, the meat  will turn into a scamp grub
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.3f },                              
                              {"guts", 0.2f },
                              {"plating", 0.5f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "twinkler",
                            ModelScale = 2.5f,
                            ModelBasicTextureName = "QuaditeRedTexture", // perhaps create special carcass items for other textures...                                         
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });
                #region leafcutter carcass
                listOfEntityTypes.Add(new EntityType("item:leafcutterCarcass")
                { 
                    Name = "Field quadite carcass",
                    SummaryDescription = "A dead field quadite", //
               //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.3f },                              
                              {"guts", 0.2f },
                              {"plating", 0.5f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "twinklerThin", //was twinkler delete this after test
                            ModelScale = 0.1f,
                            ModelBasicTextureName = "QuaditeThinTexture",
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });
                #endregion
                #region Patrician carcass
                listOfEntityTypes.Add(new EntityType("item:patricianCarcass")
                {
                    Name = "Patrician carcass",
                    SummaryDescription = "A dead patrician", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                {
                    RenderAsModelType = new RenderAsModelType() { AssetName = "patrician", ModelScale = 3f,
                        // this will show the correct frame instead of just the bind pose:
                        DefaultInfo = new AnimConditionInfo()
                        {
                            Looping = Xclna.Xna.Animation.Looping.No,
                            SpeedFactor = 0f, // freeze the anim
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                        }
                    }
                }
                });
                #endregion
                #region Whipjaw carcass
                listOfEntityTypes.Add(new EntityType("item:whipjawCarcass")
                {
                    Name = "Whipjaw carcass",
                    SummaryDescription = "A dead field whipjaw", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "snatcher",
                            ModelScale = 3f,
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });
                #endregion
                #region mudWorm carcass
                listOfEntityTypes.Add(new EntityType("item:mudWormCarcass")
                {
                    Name = "Mud worm carcass",
                    SummaryDescription = "A dead mud worm", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.9f },                              
                              {"guts", 0.1f },

                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "worm", //
                            ModelScale = 0.4f,
                            
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });
                #endregion

                #region demontreecarcass
                listOfEntityTypes.Add(new EntityType("item:demontreeCarcass")
                {
                    Name = "Dendront carcass",
                    SummaryDescription = "A dead dendront", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "demonTree",
                            ModelScale = 3.8f, // value sould correspond to value set in creatureloader

                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } }, //todo. make dead anim.
                            }
                        }
                    }
                });
                #endregion
                #region swampDemonTreeCarcass
                listOfEntityTypes.Add(new EntityType("item:swampDemonTreeCarcass")
                {
                    Name = "Swamp dendront carcass",
                    SummaryDescription = "A dead swamp dendront", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "demonTreeMoss",
                            ModelBasicTextureName = "DemonTreeMossTexture1",
                            ModelScale = 3.8f, // value sould correspond to value set in creatureloader
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } }, //todo. make dead anim.
                            }
                        }
                    }
                });
                #endregion
                #region spikePlantCarcass
                listOfEntityTypes.Add(new EntityType("item:spikePlantCarcass")
                {
                    Name = "Ursinix carcass",
                    SummaryDescription = "A dead ursinix", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "spikePlant",
                            ModelScale = 1.8f, // value should correspond to value set in creatureloader

                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } }, //todo. make dead anim.
                            }
                        }
                    }
                });
                #endregion
                #region Megapod carcass
                listOfEntityTypes.Add(new EntityType("item:megapodCarcass")
                {
                    Name = "Megapod carcass",
                    SummaryDescription = "A dead megapod", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "worm",
                            ModelScale = 1.8f, // value should correspond to value set in creatureloader

                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } }, //todo. make dead anim.
                            }
                        }
                    }
                });

                #endregion
                #region Forest guardian carcass
                listOfEntityTypes.Add(new EntityType("item:forestGuardianCarcass")
                {
                    Name = "Forest guardian carcass",
                    SummaryDescription = "A dead forest guardian", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "forestguardian",
                            ModelScale = 2.3f, 
                        // this will show the correct frame instead of just the bind pose:
                        DefaultInfo = new AnimConditionInfo()
                        {
                            Looping = Xclna.Xna.Animation.Looping.No,
                            SpeedFactor = 0f, // freeze the anim
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                        }
                    }
                    }
                });
                #endregion
                #region thunderChickenCarcass
                listOfEntityTypes.Add(new EntityType("item:thunderChickenCarcass")
                {
                    Name = "Thunder chicken carcass",
                    SummaryDescription = "A dead thunder chicken",
                    Description = "A valued source of food for humans and animals alike.",
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "thunderchicken",
                            ModelScale = 1.25f, //1.5f
                            ModelBasicTextureName = "ThunderchickenDarkTexture", //june 2015 note that this asset is used for all thunder chick types, also one which has a very different colour (the bulky)
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });
                #endregion
                #region thinThunderChickenCarcass //mp not used presently, uses same carcass as other thunder chickens
                /*       listOfEntityTypes.Add(new EntityType("item:thinThunderChickenCarcass")
                {
                    Name = "Bajingan carcass",
                    SummaryDescription = "A dead Bajingan", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    Description = "Meat is not edible by humans.",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    Category = GameData.Instance.AllItemCategories["bodies"],
                     SubstancesType = new Entities.Substances.SubstancesType()
                     {
                          SubstanceFractions = new SerializableDictionary<string,float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                     },
                    RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType()
                   {
                       AssetName = "thunderChickenThin",
                       ModelScale = 1.25f, //1.5f
                       ModelBasicTextureName = "ThunderchickenThinTexture1",
                       // this will show the correct frame instead of just the bind pose:
                        DefaultInfo = new AnimConditionInfo()
                        {
                            Looping = Xclna.Xna.Animation.Looping.No,
                            SpeedFactor = 0f, // freeze the anim
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                        }
                   }
               }
                });*/
                #endregion
                #region pygmyThunderChickenCarcass //not used
         /*       listOfEntityTypes.Add(new EntityType("item:pygmyThunderChickenCarcass")
                {
                    Name = "Pygmy thunder chicken carcass",
                    SummaryDescription = "A dead pygmy thunder chicken", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    Description = "A valued source of food for humans and animals alike.", //
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    Category = GameData.Instance.AllItemCategories["bodies"],
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "thunderchicken",
                            ModelScale = 1.25f,
                            ModelBasicTextureName = "ThunderchickenDarkTexture",
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });*/
                #endregion
                #region bulkyThunderChickenCarcass //not used
       /*         listOfEntityTypes.Add(new EntityType("item:bulkyThunderChickenCarcass")
                {
                    Name = "Studded thunder chicken carcass", //
                    SummaryDescription = "A dead studded thunder chicken", //
                    Description = "A valued source of food for humans and animals alike.", //
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    Category = GameData.Instance.AllItemCategories["bodies"],
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "thunderChickenBulky",
                            ModelScale = 1.25f,
                            ModelBasicTextureName = "ThunderchickenBulkyTexture1",
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });*/
                #endregion
                #region Binal rat carcass
                listOfEntityTypes.Add(new EntityType("item:binalRatCarcass")
                {
                    Name = "Binal rat carcass",
                    SummaryDescription = "A dead binal rat",
                    Icon = "HUD_icon_carcass",
                    Description = "The meat of this animal is inedible by humans.", 
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "raw meat",
                        DegradesTo = "item:organicMatter" // MP feb 2014: I want the rat carcasses to dissappear quickly but couldn't find out how to make them degrade to nothing???? mp: should degrade to nothing if DegradesTo is commented out.
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "thunderchicken",
                            ModelScale = 1.5f, //1.5f
                            ModelBasicTextureName = "BinalRatBrownTexture",
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });
                #endregion
                #region Dog carcass
                listOfEntityTypes.Add(new EntityType("item:dogCarcass")
                {
                    Name = "Dog carcass",
                    SummaryDescription = "A dead dog",
                    Description = "N/A", //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "dog",
                            ModelScale = 1.5f, //1.5f
                            ModelBasicTextureName = "DogGermanShepherdTexture",
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });
                #endregion
                #region Bush dragon carcass
                listOfEntityTypes.Add(new EntityType("item:bushDragonCarcass")
                {
                    Name = "Bush dragon carcass",
                    SummaryDescription = "A dead bush dragon.",
                    Description = "Inedible by humans but could find other uses.",
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType()
                   {
                       AssetName = "bushdragon",
                       ModelScale = 0.8f, 
                       ModelBasicTextureName = "BushdragonPaleTexture",
                       // this will show the correct frame instead of just the bind pose:
                        DefaultInfo = new AnimConditionInfo()
                        {
                            Looping = Xclna.Xna.Animation.Looping.No,
                            SpeedFactor = 0f, // freeze the anim
                            SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                        }
                   }
               }
                });
                #endregion
                #region Harvested bush dragon carcass
                listOfEntityTypes.Add(new EntityType("item:bushDragonHarvestedCarcass")
                {
                    Name = "Harvested bush dragon carcass",
                    SummaryDescription = "A dead bush dragon whose organs of value to us have been extracted",
                    Description = "The rest of this carcass has little use to us.",
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true /*, CarcassType = new CarcassType() { }*/ },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    SubstancesType = new Entities.Substances.SubstancesType()
                    {
                        SubstanceFractions = new SerializableDictionary<string, float>()
                          {
                              {"meat", 0.5f },
                              {"bones", 0.2f },
                              {"guts", 0.2f },
                              {"hide", 0.1f }
                          }
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsModelType = new RenderAsModelType()
                        {
                            AssetName = "bushdragon",
                            ModelScale = 0.8f,
                            ModelBasicTextureName = "BushdragonPaleTexture",
                            // this will show the correct frame instead of just the bind pose:
                            DefaultInfo = new AnimConditionInfo()
                            {
                                Looping = Xclna.Xna.Animation.Looping.No,
                                SpeedFactor = 0f, // freeze the anim
                                SoundAndAnimationSet = new RandomSoundAndAnimationSet() { BaseAnimations = new string[] { "dead" } },
                            }
                        }
                    }
                });
                #endregion
                #region Diamond bird carcass
                listOfEntityTypes.Add(new EntityType("item:birdCarcass")
                {
                    Name = "Diamond bird carcass",
                    SummaryDescription = "A dead diamond bird", //
                    //     Description = "N/A", //placeholder txt //todo
                    Icon = "HUD_icon_carcass",
                    ItemType = new ItemType() { HasNoMaximumBulk = true, CarcassType = new CarcassType() { } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "carcassDecomposing",
                        DegradesTo = "item:organicMatter" 
                    },
                    CategoryKey = "bodies",
                    RenderableType = new RenderableType()
               {
                   RenderAsModelType = new RenderAsModelType() { AssetName = "bird", ModelScale = 2.1f, ModelBasicTextureName = "BirdYellowTexture" }
               }
                });
                #endregion

                #endregion

                #region TOOLS



                #region bellows
                listOfEntityTypes.Add(new EntityType("item:bellows")
                {
                    Name = "Bellows",
                    SummaryDescription = "Simple hand tool for blowing air. Used for ventilating a furnace",
                    Description = "Consists of a flexible bag between two rigid boards. When the bag is compressed, air blows through the nozzle.", 
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "blowTool" } },
                    ItemType = new ItemType() { MaximumBulk = 0.08f},
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bellows" } } } }
                });
#endregion
                #region blowpipe
                listOfEntityTypes.Add(new EntityType("item:blowpipe")
                {
                    Name = "Blowpipe",
                    SummaryDescription = "Hollow pipe for blowing air. Useful as simple ventilation in a low temperature furnace", //
                    Description = "A very basic form of furnace ventilation that depends on the lung capacity of the person manning it.", 
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "blowTool" } },
                    ItemType = new ItemType() { MaximumBulk = 0.1f },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "pole" } } } }
                });
#endregion

                #region turnipCracker
                listOfEntityTypes.Add(new EntityType("item:turnipCracker")
                {
                    Name = "Turnip cracker",
                    SummaryDescription = "Jack-like item used for splitting a turnip's shell", //
                    Description = "Made of wrought iron. Consists of two heavy jaws connected to a notched lifting post. The jaws expand when a lever is turned in the same way that an old wagon jack functions. This makes it possible to split the sturdy shell of a turnip animal.",//not made with threaded rod. http://www.farmcollector.com/equipment/oldest-known-antique-jack-zm0z12sepzbea.aspx
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                      //  ToolTag = new[] { "openTurnipShell" },
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.2f, //heavy tool
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } },// 

                });
                #endregion



                #region diamond knife //mp why is this changed to this?
                CreateDiamondKnife(listOfEntityTypes);
                #endregion
                #region Knife (scrap metal)
                listOfEntityTypes.Add(new EntityType("item:improvisedKnife")
                {
                    Name = "Knife (scrap metal)",
                    SummaryDescription = "A makeshift knife. Blade made from scrap metal.",
                    Description = "",
                    TierOrArea = new TierOrArea() { Tier = "survival"}, 
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        ToolTag = new[] { "knife", "butcherFlesh" }
                    },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.05f, //changed by AF on 22/5-14
                        
                        AttachedObjectRenderableType = "knife",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Knife },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["improvisedKnifeHack"] }
                           // DesirabilityForUseDefensive = 0.25f
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Minor } ,
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                       // PartKeys = new SerializableDictionary<string, int>() { { "item:improvisedMetalKnifeBlade", 1}, { "item:toolHandle", 1}},                    
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "knife" } } }}
                      
                });
                #endregion

                #region Knife (flint)
                listOfEntityTypes.Add(new EntityType("item:flintKnife")
                {
                    Name = "Knife (flint)", //http://www.wilderness-survival.net/forums/archive/index.php/t-11798.html
                    SummaryDescription = "A primitive knife made from knapped flint",
                    Description = "The stone blade has been painstakingly flaked by hand, then joined with a wooden handle to form a sharp but fragile cutting tool.",
                    TierOrArea = new TierOrArea() { Tier = "survival" }, 
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        ToolTag = new[] { "knife", "butcherFlesh" }
                    },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.05f, //

                        AttachedObjectRenderableType = "knife",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Knife },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["improvisedKnifeHack"] }, //mp note that it is as good as steel knife...
                           
                            // DesirabilityForUseDefensive = 0.25f
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Minor } ,
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                        //DegradesTo =                        
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "knife" } } } },
                       
                });
                #endregion

                #region steel knife //should we also have an iron knife?
                listOfEntityTypes.Add(new EntityType("item:steelKnife")
                {
                    Name = "Knife (steel)",
                    SummaryDescription = "A durable, handmade steel knife",
                    Description = "Made with excellent craftsmanship.",
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        ToolTag = new[] { "knife", "butcherFlesh" }
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.05f, //
                        AttachedObjectRenderableType = "knife",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Knife },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["improvisedKnifeHack"] }
                            // DesirabilityForUseDefensive = 0.25f
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Minor } ,
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        DegradesTo = "item:blisterSteel"
                       // PartKeys = new SerializableDictionary<string, int>() { { "item:steelKnifeBlade", 1 }, { "item:toolHandle", 1 }, }
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "knife" } } } }                    
                });
                #endregion

                #region knife components //not used
               /* listOfEntityTypes.Add(new EntityType("item:steelKnifeBlade")
                {
                    Name = "Knife blade (steel)",
                    SummaryDescription = "Knife blade handmade from steel", 
                    Description = "When joined with a handle it makes a very durable and sharp knife.",
                    ItemType = new ItemType() { MaximumBulk = 0.05f },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } } } } //placeholder
                });
            */
            /*
                listOfEntityTypes.Add(new EntityType("item:improvisedMetalKnifeBlade")
                {
                    Name = "Knife blade",
                    SummaryDescription = "Knife blade made from scrap metal", //
                    Description = "When joined with a handle it makes a reasonably durable improvised knife.",
                    ItemType = new ItemType() { MaximumBulk = 0.05f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } } }}
                });

            */
#endregion

  
                #region machete
                listOfEntityTypes.Add(new EntityType("item:advancedMachete")
                {
                    Name = "Machete (diamondoid carbon)", // http://en.wikipedia.org/wiki/Machete
                    SummaryDescription = "Long cleaving/cutting tool. Exceptional quality", //
                    Description = "Developed for the Tau Ceti Program. Very versatile but is not well-suited for precision cutting because of its size.",
             //       Icon = "machete", //mp not used because we render the item billboard on the game area (see below)
                    ToolType = new ToolType() { Durability = 0.9f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "butcherFlesh", "cutThinShell", } },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType()
                    {
                         HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f, // changed by AF 22/5-14 was 0.2
                        
                        AnimStatesWhenAttached = new AnimModifier[]{ AnimModifier.Machete },
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AttachedObjectRenderableType = "machete",
                        WeaponType = new WeaponType()
                          {
                              AttackTypes = new AttackType[] { 
                                    GameData.Instance.AllAttackTypes["personHack"]}
                          },
                         TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "machete" } } }}
                });
                #endregion
                #region steelMachete
                listOfEntityTypes.Add(new EntityType("item:steelMachete")
                {
                    Name = "Machete (steel)", // http://en.wikipedia.org/wiki/Machete
                    SummaryDescription = "Long cleaving/cutting tool made of steel",
                    Description = "Suited for cutting down soft types of vegetation",//
                    //       Icon = "machete", //mp not used because we render the item billboard on the game area (see below)
                    ToolType = new ToolType() { Durability = 0.9f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "butcherFlesh", "cutThinShell", } },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.12f, // changed by AF 22/5-14 was 0.2

                        AnimStatesWhenAttached = new AnimModifier[] { AnimModifier.Machete },
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AttachedObjectRenderableType = "machete",
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { 
                                    GameData.Instance.AllAttackTypes["personHack"]}                          
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        DegradesTo = "item:blisterSteel"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "machete" } } } }
                });
                #endregion

               
                #region improvisedHandAxe
                listOfEntityTypes.Add(new EntityType("item:improvisedHandAxe") // mp hand axe is often used  in the meaning a one-handed axe. in archeaology, a hand axe is a prehistoric stone cutting tool without a handle ("One of the oldest tools used by man. We may have to resort to stone handaxes again if everything else fails.")
                //we should try not to confuse the player... (NB a hatchet is a hand axe with a hammer head on the other side)
                {
                    Name = "Hand axe (scrap metal)",
                    SummaryDescription = "Improvised small axe useful as a weapon and for chopping wood",
                    Description = "Made from scrap metal.",//
                    //  Icon = "hoe", //mp not used because we render the item billboard on the game area (see below)
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        ToolTag = new[] { "shapenSmallWood", "butcherFlesh", "cutThinShell" } //todo. hard wood.
                    },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f, //

                        AttachedObjectRenderableType = "axe",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Axe },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personHack"] }
                            //DesirabilityForUseDefensive = 0.3f
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "axe" } } } }

                });
                #endregion


                #region steelHandAxe
                listOfEntityTypes.Add(new EntityType("item:steelHandAxe") // mp hand axe is often used  in the meaning a one-handed axe. in archeaology, a hand axe is a prehistoric stone cutting tool without a handle ("One of the oldest tools used by man. We may have to resort to stone handaxes again if everything else fails.")
                //we should try not to confuse the player... (NB a hatchet is a hand axe with a hammer head on the other side)
                {
                    Name = "Hand axe (steel)",
                    SummaryDescription = "Small axe, very useful for chopping hard wood",
                    Description = "Made from steel, has excellent durability and sharpness",//
                    //  Icon = "hoe", //mp not used because we render the item billboard on the game area (see below)
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        ToolTag = new[] { "shapenSmallWood", "butcherFlesh", "cutThinShell" } //todo. hard wood.
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f, //

                        AttachedObjectRenderableType = "axe",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Axe },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personHack"] }
                            //DesirabilityForUseDefensive = 0.3f
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        DegradesTo = "item:blisterSteel"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "axe" } } } }

                });
#endregion


                #region improvisedPickaxe
                listOfEntityTypes.Add(new EntityType("item:improvisedPickaxe")
                {
                    Name = "Pickaxe (scrap metal)",
                    SummaryDescription = "Improvised hand tool with a pointed head",
                    Description = "Made from scrap metal, this tool is useful for loosening hard soil types and as a weapon, especially against animals with a hard shell.",//
                    //  Icon = "hoe", //mp not used because we render the item billboard on the game area (see below)
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                   //   ToolTag = new[] { "" } //not necessary. defined in toolsloader.
                    },
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.13f, //

                        AttachedObjectRenderableType = "pickaxe",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Pickaxe },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personHack"] }
                            //DesirabilityForUseDefensive = 0.3f
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        DegradesTo = "item:wroughtIron"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "pickaxe" } } } }

                });
                #endregion
                #region improvisedPickaxeHead //not used
               /* listOfEntityTypes.Add(new EntityType("item:improvisedPickaxeHead")
                {
                    Name = "Pickaxe head (improvised)",
                    SummaryDescription = "Made from scrap metal. Must be joined with a handle",//
                    Description = "N/A", //
                    ItemType = new ItemType() { MaximumBulk = 0.08f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } } //placeholder asset
                });*/
                #endregion
                #region steelPickaxe
                listOfEntityTypes.Add(new EntityType("item:steelPickaxe") 
                {
                    Name = "Pickaxe (steel)",
                    SummaryDescription = "Hand tool with a pointed head",
                    Description = "Made from steel, this tool is useful for loosening hard soil types and as a weapon, especially against animals with a hard shell.",//placeholder txt
                    //  Icon = "hoe", //mp not used because we render the item billboard on the game area (see below)
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                   //   ToolTag = new[] { "" } //not necessary. defined in toolsloader.
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.13f, //

                        AttachedObjectRenderableType = "pickaxe",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Pickaxe },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personHack"] }
                            //DesirabilityForUseDefensive = 0.3f
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Normal }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        DegradesTo = "item:blisterSteel"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "pickaxe" } } } }

                });
                #endregion


                #region hammer
              
                listOfEntityTypes.Add(new EntityType("item:hammer") 
                {
                    Name = "Hammer (iron)",
                    SummaryDescription = "A good quality hammer useful for smithing",
                    Description = "N/A",
                  //  Icon = "hammer", //not needed because the rendearble 'on ground' is defined underneath as a billboard sprite
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        ToolTag = new[] { "bluntTool" } //todo. 
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.08f, //

                        AttachedObjectRenderableType = "hammer",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Hammer },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personHammerBlow"] }
                            //DesirabilityForUseDefensive = 0.3f
                        },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.None },//todo
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None }, //todo
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.None },
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.None }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None}, //mp I'm setting this as minor/none for all...
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        DegradesTo = "item:wroughtIron"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "hammer" } } } }

                });
                #endregion
                #region stone hammer
                listOfEntityTypes.Add(new EntityType("item:stoneHammer")
                {
                    Name = "Hammer (stone)",
                    SummaryDescription = "A very crude hammer, consisting of a large rock",
                    Description = "Only useful for the most rudimentary work.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "bluntTool" } },
                    ItemType = new ItemType() { MaximumBulk = 0.25f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "dirt",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } }
                });
                #endregion

                #region file
                listOfEntityTypes.Add(new EntityType("item:file")
                {
                    Name = "File",
                    SummaryDescription = "Hand tool for shaping metal and wood. Cuts away small amounts of material",
                    Description = "A tool essential for blacksmithing",
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool,
                   // ToolTag = new[] { "" }
                    },
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } }
                });
                #endregion
                #region tongs
                listOfEntityTypes.Add(new EntityType("item:tongs")
                {
                    Name = "Tongs",
                    SummaryDescription = "Hand tool for gripping heavy and hot objects",
                    Description = "These iron tongs are essential for blacksmithing.", // The joint is placed close to the gripping ends.
                    ToolType = new ToolType()
                    {
                        Durability = 1f,
                        ToolHandling = ToolHandlingType.HandTool,
                        // ToolTag = new[] { "" }
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } }
                });
                #endregion
                #region handDrill
                listOfEntityTypes.Add(new EntityType("item:handDrill")
                {
                    Name = "Hand drill",
                    SummaryDescription = "Hand tool for boring and drilling in metal and wood",
                    Description = "", // todo
                    ToolType = new ToolType()
                    {
                        Durability = 1f,
                        ToolHandling = ToolHandlingType.HandTool,
                        // ToolTag = new[] { "" }
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } }
                });
                #endregion

                #region hackSaw
                listOfEntityTypes.Add(new EntityType("item:hacksaw")
                {
                    Name = "Hacksaw",
                    SummaryDescription = "Hand tool for sawing metal",
                    Description = "", // todo
                    ToolType = new ToolType()
                    {
                        Durability = 1f,
                        ToolHandling = ToolHandlingType.HandTool,
                        // ToolTag = new[] { "" }
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } }
                });
                #endregion

                #region bow saw
                listOfEntityTypes.Add(new EntityType("item:bowSaw")
                {
                    Name = "Bow saw",
                    SummaryDescription = "Hand tool for sawing wood",
                    Description = "", // todo
                    ToolType = new ToolType()
                    {
                        Durability = 1f,
                        ToolHandling = ToolHandlingType.HandTool,
                        // ToolTag = new[] { "" }
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } }
                });
                #endregion

                #region blacksmithstoolbox
                listOfEntityTypes.Add(new EntityType("item:blacksmithsToolbox") 
                {
                    Name = "Blacksmith's toolset",
                    SummaryDescription = "A basic collection of tools used by the blacksmith",
                    Description = "The toolbox contains a hammer, file and tongs",
                  //  Icon = "hammer", //not needed because the renderable 'on ground' is defined underneath as a billboard sprite
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                    //    ToolTag = new[] { "bluntTool" } //todo. 
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.16f, //

                        AttachedObjectRenderableType = "hammer",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Hammer },      //the hammer is always used by the blacksmith, its the most important tool in his toolbox          
        
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageBlacksmithsToolbox",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:hammer", 1 }, { "item:file", 1 }, { "item:tongs", 1 } } 
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } } //                    

                });
                #endregion

                #region metalworkersToolbox
                listOfEntityTypes.Add(new EntityType("item:metalWorkersToolbox")
                {
                    Name = "Metal worker's toolset", 
                    SummaryDescription = "A versatile collection of tools used by the metalworker and the blacksmith",
                    Description = "The toolbox contains a hammer, file, tongs, hand drill and hacksaw",
                    //  Icon = "hammer", //not needed because the renderable 'on ground' is defined underneath as a billboard sprite
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        //    ToolTag = new[] { "bluntTool" } //todo. 
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.16f, //

                        AttachedObjectRenderableType = "hammer",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Hammer },      //I'm also displaying the hammer anim with this toolset, because it is often used in the forge.          

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageMetalworkersToolbox",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:hammer", 1 }, { "item:file", 1 }, { "item:tongs", 1 }, { "item:handDrill", 1 }, { "item:hacksaw", 1 } }
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } } //                    

                });
                #endregion

                #region carpentersToolbox
                listOfEntityTypes.Add(new EntityType("item:carpentersToolbox")
                {
                    Name = "Carpenter's toolset",
                    SummaryDescription = "A versatile collection of tools used by the carpenter",
                    Description = "The toolbox contains a hammer, file, axe, hand drill and bow saw",
                    //  Icon = "hammer", //not needed because the renderable 'on ground' is defined underneath as a billboard sprite
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        //    ToolTag = new[] { "bluntTool" } //todo. 
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.16f, //

                        AttachedObjectRenderableType = "axe",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Axe },

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageCarpentersToolbox",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:hammer", 1 }, { "item:file", 1 }, { "item:steelHandAxe", 1 }, { "item:handDrill", 1 }, { "item:bowSaw", 1 } }
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } } //                    

                });
                #endregion

                #region hoe
                listOfEntityTypes.Add(new EntityType("item:farmingHoe")
                {
                    Name = "Hoe (scrap metal)", // http://en.wikipedia.org/wiki/Hoe_(tool)
                    SummaryDescription = "Improvised farming tool for weeding and tilling soil", //
                    Description = "Made from scrap metal. Sufficient for establishing a small farm plot",
                    Icon = "hoe", //uses sprite for prod. data
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "plowingTools", "unPlowingTools", "weedingTools" } },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f,

                        AnimStatesWhenAttached = new AnimModifier[] { AnimModifier.Hoe },
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AttachedObjectRenderableType = "farmingHoe",

                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Minor }, //mp not sure what this should be, compare with knife.
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None }, // don't use this when hunting unspecified prey.
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.None }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None}, //mp I'm setting this as minor/none for all...
                         },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { 
                                    GameData.Instance.AllAttackTypes["personHoeHack"]}
                        }

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                       // PartKeys = new SerializableDictionary<string, int>() {  { "item:improvisedMetalHoeBlade", 1} } // NA Commentet out not needed anymore
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { RenderAsModelType = new RenderAsModelType() { AssetName = "farmingHoe", ModelScale = 2f } }                    
                });
                
                #endregion
                #region steelHoe
                listOfEntityTypes.Add(new EntityType("item:steelHoe")
                {
                    Name = "Hoe (steel)", // http://en.wikipedia.org/wiki/Hoe_(tool)
                    SummaryDescription = "Farming tool for weeding and tilling soil", //copy from farmingHoe
                    Description = "Sufficient for establishing a small farm plot.", //copy from farmingHoe
                    Icon = "hoe",
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "plowingTools", "unPlowingTools", "weedingTools" } },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f,
                        AnimStatesWhenAttached = new AnimModifier[] { AnimModifier.Hoe },
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AttachedObjectRenderableType = "farmingHoe",
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Minor }, //mp not sure what this should be, compare with knife.
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None }, // don't use this when hunting unspecified prey.
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.None }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None}, //mp I'm setting this as minor/none for all...
                         },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["personHoeHack"] }
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        DegradesTo = "item:blisterSteel"
                       // PartKeys = new SerializableDictionary<string, int>() { { "item:steelHoeBlade", 1 }, }
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { RenderAsModelType = new RenderAsModelType() { AssetName = "farmingHoe", ModelScale = 2f } },
                    
                });
                #endregion
 
                #region improvisedSpade
                listOfEntityTypes.Add(new EntityType("item:improvisedSpade")
                {
                    Name = "Spade (scrap metal)", //looks more like a shovel actually. rename?
                    SummaryDescription = "Spade made from improvised materials",
                    Icon = "shovel", //the sprite png for production info
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        ToolTag = new[] { "plowingTools", "unPlowingTools", "diggingSoil" } 
                    },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.1f,

                        AttachedObjectRenderableType = "shovel",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Shovel },

                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.None }, //mp not sure what this should be, compare with knife.
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None }, // don't use this when hunting unspecified prey.
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.None }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None}, //mp I'm setting this as minor/none for all...
                         },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { 
                                    GameData.Instance.AllAttackTypes["personShovelHack"]}
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",

                    RenderableType = new RenderableType() { RenderAsModelType = new RenderAsModelType() { AssetName = "shovel", ModelScale = 2f } },


                //    PartKeys = new SerializableDictionary<string, int>() { { "item:flintSpearhead", 1 } } //todo
                });
                #endregion

                #region steelSpade
                listOfEntityTypes.Add(new EntityType("item:steelSpade")
                {
                    Name = "Spade (steel)", //looks more like a shovel actually. rename?
                    SummaryDescription = "Spade with a steel blade", //
                    Description = "Very useful for digging. Has excellent durability and sharpness.", //
                    Icon = "shovel", //the sprite png for production info

                    ToolType = new ToolType()
                    {
                        Durability = toolDurabilityDurable,
                        ToolHandling = ToolHandlingType.HandTool,
                        ToolTag = new[] { "plowingTools", "unPlowingTools", "diggingSoil" } 
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.125f,

                        AttachedObjectRenderableType = "shovel",
                        AttachorTagToMountOn = "rightHand",
                        AttachesToBodyPart = "Right arm",
                        AnimStatesWhenAttached = new[] { AnimModifier.Shovel },

                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.None }, //mp not sure what this should be, compare with knife.
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None }, // don't use this when hunting unspecified prey.
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.None }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None}, //mp I'm setting this as minor/none for all...
                         },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { 
                                    GameData.Instance.AllAttackTypes["personShovelHack"]}
                        }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        DegradesTo = "item:blisterSteel"
                    },
                    CategoryKey = "tools",

                    RenderableType = new RenderableType() { RenderAsModelType = new RenderAsModelType() { AssetName = "shovel", ModelScale = 2f } },


                    //    PartKeys = new SerializableDictionary<string, int>() { { "item:flintSpearhead", 1 } } //todo
                });
                #endregion

                #region improvised trowel
                listOfEntityTypes.Add(new EntityType("item:improvisedTrowel")
                {
                    Name = "Trowel (improvised)",
                    SummaryDescription = "Very simple digging tool, consisting of a flat stick",
                    Description = "When lacking a spade, this stick can be used for loosening hard soil before moving it by hand",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "diggingSoil" } }, 
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } } } }
                });
                #endregion

                #region blunt knife
                listOfEntityTypes.Add(new EntityType("item:bluntKnife")
                {
                    Name = "Blunt knife",
                    SummaryDescription = "Wooden tool for scraping and shaping soft materials",
                    Description = "Useful for shaping pottery and scraping animal hides, for example.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "bluntKnife" } },
                    ItemType = new ItemType() { MaximumBulk = 0.07f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } } } }
                });
                #endregion



      

                #region vinegar //this is both used as material (pickling) and tool (for making more vinegar). todo stats in spreadsheet
                listOfEntityTypes.Add(new EntityType("item:vinegar")
                {
                    Name = "Vinegar",
                    SummaryDescription = "A mild, edible acid useful for preservation of food", //
                    Description = "Like on Earth, many naturally occuring bacteria here can produce vinegar by fermentation of fruit. The vinegar can be used for conserving food (pickling). When making more vinegar, the production is greatly sped up by using a preexisting batch of vinegar (bacterial culture) to kickstart the process.",
                    ToolType = new ToolType() { Durability = 0.2f, ToolHandling = ToolHandlingType.HandTool }, //tool for making more vinegar. also used as material input for pickling.
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.125f,
                        RequiredStorageTags = new[] { "storageTagLiquidContainerClosedNoHeat" },

                        FoodType = new FoodType() { FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorVegetables"], FoodTags = new string[] { "inedibleIngredient" } }  //no animals or humans want to eat it
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "pickledFood",
                        DegradesTo = "item:organicMatter"
                    },
                    CategoryKey = "ingredients",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "vegetables" } } } } //todo
                });
                #endregion

                #region shadeleafResin
                listOfEntityTypes.Add(new EntityType("item:shadeleafResin")
                {
                    Name = "Shadeleaf resin",
                    SummaryDescription = "A resinous gum which works a glue and protects materials from bug infestation.", //http://en.wikipedia.org/wiki/List_of_glues  http://en.wikipedia.org/wiki/Canada_balsam
                    Description = "Sticky, colourless gum that turns to a transparent mass when the essential oils have been allowed to evaporate. Can be used for joining light objects. Also has a repellent effect on scuttler bugs and can be applied to plant material to protect against infestation",
                    TierOrArea = new TierOrArea() { Tier = "survival"},
                    ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "improvisedGlue", "combineLightImprovisedObjects" } },
                    ItemType = new ItemType() { MaximumBulk = 0.07f, }, 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "improvisedEquipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }},

                });
                #endregion       


                #region string
                listOfEntityTypes.Add(new EntityType("item:advancedString")
                {
                    Name = "String (high-tech)",
                    SummaryDescription = "Ultra strong and lightweight string", //http://www.stringforum.net/stringdb.php
                    Description = "Among the Tau Ceti Mission standard equipment. Its multifilament construction offers extreme power and durability and high tensile strength.",
                    TierOrArea = new TierOrArea() { Tier = "advanced"},
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "lightString", "cordage" }},  //used for cordage in shelters
                    ItemType = new ItemType() { MaximumBulk = 0.04f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wire" } } }}
                });
            #endregion
                #region metalWire
                listOfEntityTypes.Add(new EntityType("item:metalWire")
                {
                    Name = "Metal wire",
                    SummaryDescription = "A small roll of metal wire", // 
                    Description = "Wraps around and stays in place - should be useful for all kinds of small construction tasks",
                    TierOrArea = new TierOrArea() { Tier = "medium" },
                    ToolType = new ToolType() { Durability = 0.9f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "lightString" } },
                    ItemType = new ItemType() { MaximumBulk = 0.04f, },

                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wire" } } }}
                });
#endregion
                #region rawhideString
                listOfEntityTypes.Add(new EntityType("item:rawhideString")
                {
                    Name = "String (rawhide)",
                    SummaryDescription = "Crude string made from rawhide", //
                    Description = "Adequate for simple construction tasks. Stiff when dry.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "lightString", "cordage" } },  //used for cordage in shelters
                    ItemType = new ItemType() { MaximumBulk = 0.05f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "improvisedEquipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wire" } } } }
                });
#endregion

                #region cottonString
                listOfEntityTypes.Add(new EntityType("item:cottonString")
                {
                    Name = "String (cotton)",
                    SummaryDescription = "Sturdy string made from cotton", //
                    Description = "Can be used both as a tool for small crafting tasks and as material for a fishing net.",
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "lightString", "cordage" } },  //used for cordage in shelters
                    ItemType = new ItemType() { MaximumBulk = 0.05f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wire" } } } }
                });
                #endregion

                #region vine
                listOfEntityTypes.Add(new EntityType("item:vine")
                {
                    Name = "Vine",
                    SummaryDescription = "Rope-like stems of a climbing plant", //http://en.wikipedia.org/wiki/Vine
                    Description = "This vine is reasonably durable and strong and makes a natural cordage that can be used in shelter construction.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = 0.2f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] {"cordage" }},  //used for cordage in shelters
                    ItemType = new ItemType() { MaximumBulk = 0.07f }, //changed so that it doesnt weigh the same as metal tools
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } }}}
                });
#endregion
                #region paracord //NOT USED
                /*          listOfEntityTypes.Add(new EntityType("item:paracord")
                {
                    Name = "Paracord",
                    ToolType = new ToolType() { Durability = 0.9f, ToolHandling = ToolHandlingType.HandTool , ToolTag = new[] { "cordage" }},  //used for cordage in shelters
                    ItemType = new ItemType() { MaximumBulk = 0.1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    Category = GameData.Instance.AllItemCategories["tools"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wire" } }}}
                });*/
                #endregion
                #region superconductingWire
                listOfEntityTypes.Add(new EntityType("item:superconductingWire")
                {
                    Name = "Superconducting wire",
                    SummaryDescription = "Pieces of electrical wiring with very low resistance.", //. http://en.wikipedia.org/wiki/Wire
                    Description = "Although designed for electric power transmission, this wire has enough ductility and tensile strength that it may find use in simple construction tasks.", 
                    ToolType = new ToolType() { Durability = 0.9f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "lightString", "cordage" }},
                    ItemType = new ItemType() { MaximumBulk = 0.04f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "wire" } } }}
                });
#endregion
                    #region brickMold
                    listOfEntityTypes.Add(new EntityType("item:brickMold")
                    {
                        Name = "Brick mold",
                        SummaryDescription = "A simple wooden mold used for shaping rectangular mudbricks",
                        Description = "A very simple tool, basically a frame made from wood",
                        ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "mold" } },
                        ItemType = new ItemType() { MaximumBulk = 0.06f },
                        NonLivingType = new NonLivingType()
                        {
                            Repairability = 1f,
                            DegradeType = "equipment",
                        },
                        TierOrArea = new TierOrArea() { Tier = "basic" },
                        CategoryKey = "tools",
                        RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "brickMold" } } } }
                    });
                    #endregion

                    #region bulletMold
                    listOfEntityTypes.Add(new EntityType("item:bulletMold")
                    {
                        Name = "Bullet mold",
                        SummaryDescription = "A simple ceramic mold used for casting bullets",//
                        Description = "The ceramic is made from specially selected clay and can withstand the temperature of melted gold. The hot, molten metal is poured into the holes in the mold and allowed to cool. This method produces bullets of a sufficient quality.",//
                        ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool},
                        ItemType = new ItemType() { MaximumBulk = 0.09f },
                        NonLivingType = new NonLivingType()
                        {
                            Repairability = 1f,
                            DegradeType = "equipment",
                        },
                        TierOrArea = new TierOrArea() { Tier = "basic" },
                        CategoryKey = "tools",
                        RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } }//placeholder asset
                    });
                    #endregion

                    #region sandMold
                    listOfEntityTypes.Add(new EntityType("item:sandMold")
                    {
                        Name = "Sand mold",
                        SummaryDescription = "Mold used for sand casting metal",
                        Description = "A wooden frame (also called a flask) filled with a temperature resistant molding sand (greensand). The flask is used together with a casting pattern to form a mold into which the hot metal is poured.",
                        ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "mold" } },
                        ItemType = new ItemType() { MaximumBulk = 0.09f },
                        NonLivingType = new NonLivingType()
                        {
                            Repairability = 1f,
                            DegradeType = "improvisedEquipment",
                        },
                        TierOrArea = new TierOrArea() { Tier = "basic" },
                        CategoryKey = "tools",
                        RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "brickMold" } } } }
                    });
                    #endregion

            

                    #region Ursinix venom gland
                    listOfEntityTypes.Add(new EntityType("item:ursinixVenomGland")
                    {
                        Name = "Ursinix venom gland",
                        SummaryDescription = "This animal organ contains a large amount of strong neurotoxin",
                        Description = "The poison contained in this tissue might find use for us, but it should be handled with great care. This rapid-acting poison could be used when fishing for small fish by throwing it in the water.",
                        TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                        ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool },
                        ItemType = new ItemType() { MaximumBulk = 0.05f },
                        NonLivingType = new NonLivingType()
                        {
                            Repairability = 0f,
                            DegradeType = "perishable",
                            DegradesTo = "item:organicMatter"
                        },
                        CategoryKey = "rawMaterials",
                        RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } } }
                    });
                    #endregion
                    # region ////Fish bait
                    listOfEntityTypes.Add(new EntityType("item:neonHornetsLive")
                    {
                        Name = "Live neon hornets",
                        SummaryDescription = "Flying insects that emit a strong glow",
                        Description = "\n BIOLOGY OVERVIEW\n Neon hornets are aggressive, hive-building insects with thoracic bioluminescence.\n \nSURVIVAL GUIDE NOTES\n Their bioluminescence resembles that of the sparkscale, a small fish species that is the primary prey of the carbon tail. Hence, the neon hornet might be useful as bait to catch the carbon tail.",
                        TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food }, 
                        ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool },
                        ItemType = new ItemType() { MaximumBulk = 0.05f },
                        NonLivingType = new NonLivingType()
                        {
                            Repairability = 0f,
                            DegradeType = "raw seafood",
                            DegradesTo = "item:neonHornetsDead"
                        },
                        CategoryKey = "rawMaterials",
                        RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }}
                    });


                    listOfEntityTypes.Add(new EntityType("item:neonHornetsDead")
                    {
                        Name = "Dead neon hornets",
                        SummaryDescription = "These dead insects no longer glow",
                        Description = "Can be used as bait for fishing. We've found that the fish 'carbon tail' has a preference for neon hornets",
                        TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                        ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool },
                        ItemType = new ItemType() { MaximumBulk = 0.03f },
                        NonLivingType = new NonLivingType()
                        {
                            Repairability = 0f,
                            DegradeType = "perishable",
                            DegradesTo = "item:organicMatter"
                        },
                        CategoryKey = "rawMaterials",
                        RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }}
                    });


                    listOfEntityTypes.Add(new EntityType("item:pigFliesLive")
                    {
                        Name = "Live pig flies",
                        SummaryDescription = "Flying insect often found on copperfern",
                        Description = "The pig fly is named for the unusual snout-like sense organ it uses to find honeydew amongst the copperferns it frequently inhabits. Its quick, dance-like movement above water surfaces attracts the streak fin, which feeds on flying insects.",
                        TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                        ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool },
                        ItemType = new ItemType() { MaximumBulk = 0.05f },
                        NonLivingType = new NonLivingType()
                        {
                            Repairability = 0f,
                            DegradeType = "raw seafood",
                            DegradesTo = "item:pigFliesDead"
                        },
                        CategoryKey = "rawMaterials",
                        RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }}
                    });


                    listOfEntityTypes.Add(new EntityType("item:pigFliesDead")
                    {
                        Name = "Dead pig flies",
                        SummaryDescription = "Can be used as fishing bait", 
                        Description = "We've found that the fish streak fin has a preference for pig flies",
                        TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food }, 
                        ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool },
                        ItemType = new ItemType() { MaximumBulk = 0.03f },
                        NonLivingType = new NonLivingType()
                        {
                            Repairability = 0f,
                            DegradeType = "perishable",
                            DegradesTo = "item:organicMatter"
                        },
                        CategoryKey = "rawMaterials",
                        RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }}
                    });
                    #endregion
                    #region exaGlue
                    listOfEntityTypes.Add(new EntityType("item:exaGlue")
                {
                    Name = "Exa glue",
                    SummaryDescription = "Ultra strong adhesive", //http://en.wikipedia.org/wiki/Glue
                    Description = "Issued for the Tau Ceti Mission to aid in smaller construction tasks. Will bind almost all surfaces.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "combineLightImprovisedObjects" } },
                    ItemType = new ItemType() { MaximumBulk = 0.02f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } }},

                });
                #endregion


                    #region smoothSandstone
                    listOfEntityTypes.Add(new EntityType("item:smoothSandstone")
                {
                    Name = "Sharpening stones",
                    SummaryDescription = "Smooth sedimentary rocks suited for sharpening metal", //
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "sharpenBlade" } },
                    ItemType = new ItemType() { MaximumBulk = 0.1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } } }},


                });
            #endregion
 
                    #region powerGrinder //NOT USED
         /*           listOfEntityTypes.Add(new EntityType("item:powerGrinder")
                {
                    Name = "Power grinder",
                    SummaryDescription = "A tool like this would come in handy",
                    ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "sharpenBlade" } },
                    ItemType = new ItemType() { MaximumBulk = 0.3f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    Category = GameData.Instance.AllItemCategories["tools"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "powertool" } } }},


                });*/
            #endregion
                    #region item:metalCutter
                    listOfEntityTypes.Add(new EntityType("item:metalCutter")
                {
                    Name = "Metal cutter",
                    SummaryDescription = "A tool like this would come in handy",
                    ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.Stationary, ToolTag = new[] { "cutMetal" } },
                    ItemType = new ItemType() { MaximumBulk = 0.3f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "powertool" } } }},

                });
            #endregion
                    #region tinnerSnips
                    listOfEntityTypes.Add(new EntityType("item:tinnerSnips")
                {
                    Name = "Tinner snips",
                    SummaryDescription = "Simple, steel hand tool for cutting metal", //http://en.wikipedia.org/wiki/Snips
                    Description = "A type of scissors with long handles and short strong blades, able to cut thin metal sheets and small metal objects.",
                    ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "cutMetal" } },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, }, //
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "ironTools" } } } },

                });
            #endregion
                    #region snips
                    listOfEntityTypes.Add(new EntityType("item:advancedSnips")
                {
                    Name = "Metal shears",
                    SummaryDescription = "Advanced hand tool for cutting metal", //http://en.wikipedia.org/wiki/Snips
                    Description = "Part of the Tau Ceti Mission equipment, this tool can cut both soft and tough metal types.", 
                    ToolType = new ToolType() { Durability = toolDurabilityDurable, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "cutMetal" } },
                    ItemType = new ItemType() { MaximumBulk = 0.07f, }, //
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "knife" } } }},

                });
            #endregion
                    # region sentry Laser 
            // can shoot without ammo. might need new animations trough??
                    listOfEntityTypes.Add(new EntityType("item:sentryLaserGun")
                    {
                        Name = "Stationary machine gun component", //was: "Sentry machine gun" but I think it's too confusing with too many items named sorta the same. MP
                        SummaryDescription = "small laser for the sentry robot", //
                        Description = "The weapon component of the sentry turret",

                        ItemType = new ItemType()
                        {
                            HauledItemValue = ItemType.HauledItemValues.MostValuable,
                            MaximumBulk = 0.2f,
                            TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>() // Lars added this for use by the guardBot
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Best }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best }, 
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Best }, 
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None}, 
                         },
                            WeaponType = new WeaponType()
                            {
                                AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["sentryLaserGunShot"] },
                                IsIntrinsic = true  // no wielding by characters                          
                            }
                        },
                        TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                        // commentet out as ammo is not needed in the laser gun.
                     /*   ContainerType = new MagazineContainerType()
                        {
                            CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside

                            MaxCapacity = 100,
                            UsesAmmoTypeKeyName = "item:sentryGunAmmo",

                            ReplenishProcess = "reloadSentryGun"

                        },*/
                        NonLivingType = new NonLivingType()
                        {
                            Repairability = 1f,
                            DegradeType = "equipment"
                        },
                        CategoryKey = "rawMaterials",
                        RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } } }

                    });

                    #endregion

                # region sentry gun
                listOfEntityTypes.Add(new EntityType("item:sentryGun")
                {
                    Name = "Stationary machine gun component", //was: "Sentry machine gun" but I think it's too confusing with too many items named sorta the same. MP
                    SummaryDescription = "Machinegun for the sentry robot", //
                    Description = "The weapon component of the sentry turret",

                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.2f,
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>() // Lars added this for use by the guardBot
                         {                           
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Best }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Best }, 
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Best }, 
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None}, 
                         },
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["sentryGunBurst"] },
                            IsIntrinsic = true  // no wielding by characters                          
                        }                        
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside

                        MaxCapacity = 100, 
                        UsesAmmoTypeKeyName = "item:sentryGunAmmo",

                        ReplenishProcess = "reloadSentryGun"                            
                          
                     },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } } }

                });

                #endregion

                # region sentry spraygun
                listOfEntityTypes.Add(new EntityType("item:sentrySprayGun")
                {
                    Name = "Stationary spray gun component", //was: "Sentry spray gun" but I think it's too confusing with too many items named sorta the same. MP
                    SummaryDescription = "Improvised spray gun for the sentry turret", //
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    Description = "With some clever engineering, this can be fitted onto the sentry robot",

                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.2f,
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["shootImprovedFireExtinguisherBushDragonPoison"] },
                            IsIntrinsic = true  // no wielding by characters                          
                        }
                    },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside

                        MaxCapacity = 10, //mp must be same number as MaxNoOfRounds defined in ammo item. else they will not load.
                        UsesAmmoTag = "fireExtinguisherAmmo",

                        ReplenishProcess = "reloadSentryGun"

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } } }

                });
                #endregion

                # region sentry shotgun
                listOfEntityTypes.Add(new EntityType("item:sentryShotgun")
                {
                    Name = "Mountable shotgun", 
                    SummaryDescription = "Shotgun, modified for the sentry turret", //
                    Description = "With some minor adjustments, this weapon can be fitted onto the sentry robot",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.2f,
                        WeaponType = new WeaponType()
                        {
                            AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["shootShotgun"] },
                            IsIntrinsic = true  // no wielding by characters                          
                        }
                    },
                    ContainerType = new MagazineContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact" }, // needed to see/unsee ammo inside

                        MaxCapacity = 8, // 20, //mp must be same number as MaxNoOfRounds defined in ammo item. else they will not load.
                        UsesAmmoTypeKeyName = "item:shotgunAmmo",

                        ReplenishProcess = "reloadSentryGun"

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } } }

                });
                #endregion
                #region robot weeding tool
                listOfEntityTypes.Add(new EntityType("item:weedingRobotTool")
                {
                    Name = "GOPHER robot tool system", //was: "Anti-pest system"
                    SummaryDescription = "Robot mounted tools for harvesting and weeding", //
                    Description = "Agricultural robot tool suite for harvesting crops, weeding and controlling pests",
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.Intrinsic, ToolTag = new[] { "weedingTools", } },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.2f,                                               
                        
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } }}                   
                    
                });
                # endregion
                #region robot digging tool
                listOfEntityTypes.Add(new EntityType("item:diggingRobotTool")
                {
                    Name = "Digging system",
                    SummaryDescription = "Robot mounted system for digging", //
                    Description = "Digging robot tool suite",
                    ToolType = new ToolType() { Durability = 1f, ToolHandling = ToolHandlingType.Intrinsic, ToolTag = new[] { "miningTools", } },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType()
                    {
                        HauledItemValue = ItemType.HauledItemValues.MostValuable,
                        MaximumBulk = 0.2f,

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } } }

                });
                # endregion

                #region Pots and containers (tools)

                #region cookingPot
                listOfEntityTypes.Add(new EntityType("item:advancedCookingPot")
                {
                    Name = "Cooking pot (titanium)",
                    SummaryDescription = "Light weight metal pot",
                    Description = "Made of titanium, this cooking tool is an essential part of the standard issue Tau Ceti equipment.",
                    ToolType = new ToolType() { ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage, ToolTag = new[] { "cookingPot" } },
                    ItemType = new ItemType() { MaximumBulk = 0.09f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Food },
                    ContainerType = new ToolContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, // is an open container. add more?
                        ProductionOutputStorageType = new ItemStorageType(0.2f)
                        {
                            FullStatePercentage = 0.1f, ////used for the "full"/"empty"sprite change

                        }                           
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "pot",
                            }
                        }
                        },
                        ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "potFull"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         },
                    }
                    },
                });
                #endregion
                #region improvisedCookingPot
                listOfEntityTypes.Add(new EntityType("item:improvisedCookingPot")
                {
                    Name = "Cooking pot (scrap metal)",
                    SummaryDescription = "Metal pot made from scrap metal", //
                    Description = "An improvised metal container for cooking.",
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { ToolTag = new[] { "cookingPot" }, ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.7f,
                        DegradeType = "equipment"
                    },
                    ContainerType = new ToolContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, // is an open container. add more?
                        ProductionOutputStorageType = new ItemStorageType(0.2f)
                        {
                            FullStatePercentage = 0.1f, ////used for the "full"/"empty"sprite change

                        }
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "pot",
                            }
                        }
                        },
                        ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "potFull"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         },
                    }
                    },
                });
                #endregion
                #region goldPot
                listOfEntityTypes.Add(new EntityType("item:goldPot")
                {
                    Name = "Cooking pot (gold)",
                    SummaryDescription = "Cooking pot made from cast gold",
                    Description = "Gold is plentiful on our planet and can be used for mundane items such as this. It's a heavy, good quality cooking pot which benefits from the special properties of gold.",
                    ToolType = new ToolType() { ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage, ToolTag = new[] { "cookingPot" } },
                    ItemType = new ItemType() { MaximumBulk = 0.14f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ContainerType = new ToolContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, // is an open container. add more?
                        ProductionOutputStorageType = new ItemStorageType(0.2f)
                        {
                            FullStatePercentage = 0.1f, ////used for the "full"/"empty"sprite change

                        }
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "potGoldenEmpty", //
                            }
                        }
                        },
                        ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "potGoldenFull"}}, //
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         },
                    }
                    },
                });
                #endregion
                #region woodenCookingPot
                listOfEntityTypes.Add(new EntityType("item:woodenCookingPot")
                {
                    Name = "Cooking pot (wooden)",
                    SummaryDescription = "Cooking pot made from plant materials. Less efficient than a metal pot.",
                    Description = "This wooden vessel will burn if placed directly on a fire. Instead, food can be cooked in the pot by gradually placing hot stones in the pot together with water and ingredients. Cooking food this way requires a steady supply of stones heated in a nearby fire.", //mp: stone method not supported by our tooltags:   "The rocks need to be of the right size and type for this to work."
                    TierOrArea = new TierOrArea() { Tier = "survival" }, 
                    ToolType = new ToolType() { ToolTag = new[] { "cookingPot" }, ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.7f,
                        DegradeType = "equipment"
                    },
                    ContainerType = new ToolContainerType()
                    {
                        CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, // is an open container. add more?
                        ProductionOutputStorageType = new ItemStorageType(0.2f)
                        {
                            FullStatePercentage = 0.1f, ////used for the "full"/"empty"sprite change

                        }
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "woodenPot",
                            }
                        }
                        },
                        ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "woodenPotFull"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         },
                    }
                    },
                });
                #endregion
                #region clayPotUnglazed
                listOfEntityTypes.Add(new EntityType("item:clayPotUnglazed")
                {
                    Name = "Cooking pot (unglazed clay)",
                    SummaryDescription = "A small clay pot for cooking",
                    Description = "This clay item has low quality but can be used for cooking if care is taken during use. Since it has not been glazed it is not ideal for storing liquids.", //
                    ToolType = new ToolType() { ToolTag = new[] { "cookingPot", "liquidContainerNoHeat" }, ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.7f,
                        DegradeType = "equipment"
                    },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ContainerType = new ToolContainerType()
                    {
                      //  StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat" }, //mp this pot is not meant for storage, only cooking
                        CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, // is open container.
                        ProductionOutputStorageType = new ItemStorageType(0.2f) // capacity. same as other cookingpots
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "clayPotSmall" } } } }
                });
                #endregion
                #region vat
                listOfEntityTypes.Add(new EntityType("item:vat")
                {
                    Name = "Vat",
                    SummaryDescription = "A simple container for storing liquid", //vessel
                    Description = "Made from improvised materials.", //
                    TierOrArea = new TierOrArea() { Tier = "basic" }, // i think..
                    ToolType = new ToolType() { ToolTag = new[] { "liquidContainerNoHeat" }, ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.7f,
                        DegradeType = "equipment"
                    },
                    ContainerType = new ToolContainerType()
                    {
                        StorageTags = new[] { "storageTagLiquidContainerNoHeat" },
                        CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, // is an open container. add more?
                        ProductionOutputStorageType = new ItemStorageType(0.8f) // capacity. make sure there's room enough for the product.
                        {
                            FullStatePercentage = 0.1f, //used for the "full"/"empty"sprite change
                            
                        }    
                    },
                    CategoryKey = "tools",

                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]
                        {
                            new RenderAsBillboardType()
                            {
                                AssetName = "vatSpoakEmpty",
                            }
                        }
                        },
                        ClientStateConditions = new[]
                    {
                         new ClientStateInfo()
                         {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "vatSpoakFullFine"}},
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Full)
                         },
                    }
                    },
                });
                #endregion
                #region tappingBucket
                listOfEntityTypes.Add(new EntityType("item:tappingBucket")
                {
                    Name = "Tapping bucket",
                    SummaryDescription = "A container and a spout for tapping liquid from a tree",
                    Description = "Made from a clay jar which can be retrieved by salvaging this item.", //
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ToolType = new ToolType() { ToolTag = new[] { "liquidContainerNoHeat" }, ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.7f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageTappingBucket",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:clayJar", 1 } }
                    },
                    ContainerType = new ToolContainerType()
                    {
                        StorageTags = new[] { "storageTagLiquidContainerNoHeat" },
                        CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, // is an open container.
                        ProductionOutputStorageType = new ItemStorageType(0.4f) // capacity. make sure there's room enough for the product.
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "clayPot" } } } } //todo, make sprite
                    
                });
                #endregion
                #region plasticTappingBucket
                listOfEntityTypes.Add(new EntityType("item:plasticTappingBucket")
                {
                    Name = "Tapping bucket (plastic)",
                    SummaryDescription = "A plastic container and a spout for tapping liquid from a tree",
                    Description = "Made from a plastic jar which can be retrieved by salvaging this item.", //
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ToolType = new ToolType() { ToolTag = new[] { "liquidContainerNoHeat" }, ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.7f,
                        DegradeType = "equipment",
                        SalvageProcess = "salvagePlasticTappingBucket",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:improvisedPlasticJar", 1 } }
                    },
                    ContainerType = new ToolContainerType()
                    {
                        StorageTags = new[] { "storageTagLiquidContainerNoHeat" },
                        CanTransactWithTags = new[] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" }, // is an open container.
                        ProductionOutputStorageType = new ItemStorageType(0.4f) // capacity. make sure there's room enough for the product.
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "canister" } } } } //todo, make bigger?
                    
                });
                #endregion

                #region clayJar
                listOfEntityTypes.Add(new EntityType("item:clayJar")
                {
                    Name = "Clay jar",
                    SummaryDescription = "A closed container for storing food and liquid",
                    Description = "This clay item has been glazed by firing it in the kiln together with a handful of salt, making it waterproof.", //
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    ToolType = new ToolType() { ToolTag = new[] { "liquidContainerNoHeat" }, ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.7f,
                        DegradeType = "equipment"
                    },
                    ContainerType = new ToolContainerType()
                    {
                        StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat" },
                        CanTransactWithTags = new[] { "humanTransact" }, // is closed container.
                        ProductionOutputStorageType = new ItemStorageType(0.4f) // capacity. same as plasticjar. make sure there's room enough for the product.
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "clayPot" } } } }
                });
                #endregion
                #region scrapJar
                listOfEntityTypes.Add(new EntityType("item:improvisedPlasticJar")
                {
                    Name = "Plastic jar (improvised)",
                    SummaryDescription = "A closed container for storing food and liquid, made from plastic scraps",
                    Description = "N/A", //
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { ToolTag = new[] { "liquidContainerNoHeat" }, ToolHandling = ToolHandlingType.Stationary, Durability = toolDurabilityAverage },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.7f,
                        DegradeType = "equipment"
                    },
                    ContainerType = new ToolContainerType()
                    {
                        StorageTags = new[] { "storageTagLiquidContainerClosedNoHeat" },
                        CanTransactWithTags = new[] { "humanTransact" }, // is closed container.
                        ProductionOutputStorageType = new ItemStorageType(0.4f) // capacity. same as clayjar. make sure there's room enough for the product.
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "canister" } } } } //todo make bigger? like clayjar
                });
                #endregion


                #endregion

                #region strongBugNet
                listOfEntityTypes.Add(new EntityType("item:strongBugNet")
                {
                    Name = "Bug net",
                    SummaryDescription = "Used for catching small flyers",
                    Description = "The net consists of permeable textile and a long handle.", //
                    TierOrArea = new TierOrArea() { Tier = "survival" },
                    ToolType = new ToolType() { Durability = 0.4f, ToolHandling = ToolHandlingType.HandTool },
                    ItemType = new ItemType() { MaximumBulk = 0.08f }, //changed by AF 22/5 from 0.15 bulk
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                        
                      //  PartKeys = new SerializableDictionary<string, int>() {  { "item:textile", 1}}
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "bushcraftComponentsSmall" } }}},
                    
                });
                #endregion
                #region fishing hooks
                listOfEntityTypes.Add(new EntityType("item:thornHooks")
                {
                    Name = "Fishing hooks (thorn)",
                    SummaryDescription = "Used for fishing together with bait and string",
                    Description = "Sharp, curving thorns made into fishing hooks",
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                    ToolType = new ToolType() { Durability = 0.4f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "fishingHook" }},
                    ItemType = new ItemType() { MaximumBulk = 0.02f, },
                    
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }}                                         
                  
                });

                listOfEntityTypes.Add(new EntityType("item:improvisedMetalHooks")
                {
                    Name = "Fishing hooks (scrap metal)",
                    SummaryDescription = "Used for fishing together with bait and string",
                    Description = "Pieces of metal made into fishing hooks",
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                    ToolType = new ToolType() { Durability = 0.4f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "fishingHook" } },
                    ItemType = new ItemType() { MaximumBulk = 0.02f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }}

                });

                listOfEntityTypes.Add(new EntityType("item:woodenHooks")
                {
                    Name = "Fishing hooks (wooden)",
                    SummaryDescription = "Used for fishing together with bait and string",
                    Description = "Pieces of hard wood sharpened into fishing hooks",
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                    ToolType = new ToolType() { Durability = 0.4f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "fishingHook" } },
                    ItemType = new ItemType() { MaximumBulk = 0.02f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }}

                });
/*  //mp removed. waste of space.
                listOfEntityTypes.Add(new EntityType("item:chitinousHooks")
                {
                    Name = "Fishing hooks (chitin)",
                    SummaryDescription = "Used for fishing together with bait and string",
                    Description = "Pieces of chitinous shell made into fishing hooks",
                    TierOrArea = new TierOrArea() { Tier = "survival", Area = RatingTypes.Food },
                    ToolType = new ToolType() { Durability = 0.4f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "fishingHook" } },
                    ItemType = new ItemType() { MaximumBulk = 0.02f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    Category = GameData.Instance.AllItemCategories["tools"],
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } }}

                });
*/
                listOfEntityTypes.Add(new EntityType("item:ironHooks")
                {
                    Name = "Fishing hooks (iron)",
                    SummaryDescription = "Used for fishing together with bait and string",
                    Description = "Various fishing hooks made from wrought iron",
                    TierOrArea = new TierOrArea() { Tier = "basic", Area = RatingTypes.Food },
                    ItemType = new ItemType() { MaximumBulk = 0.02f },
                    ToolType = new ToolType() { Durability = 0.4f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "fishingHook" } },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "leatherPouch" } } } } //
                });

                #endregion



                #region molecular assembler Tools

                float assemblerPlateBulk = 0.1f;
                listOfEntityTypes.Add(new EntityType("item:assemblerPlateA")
                {
                    Name = "Assembler plate #A",
                    SummaryDescription = "Production plate for the molecular assembler",
                    Description = "Produces simple items with a diamondoid, stiff structure like: knife, machete and axe. \nLike all the assembler plates, it is vulnerable to background radiation and should be stored in a shielded container.",
                    TierOrArea = new TierOrArea() { Tier = "advanced"},
                    ToolType = new ToolType() { ToolHandling = ToolHandlingType.Stationary, Durability = assemblerPlateDurability },
                    ItemType = new ItemType() { MaximumBulk = assemblerPlateBulk, },
                    NonLivingType = new NonLivingType()
                    {                       
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "nanoplate" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:masterAssemblerPlateA")
                {
                    Name = "Master assembler plate #A",
                    SummaryDescription = "Replicating plate for the molecular assembler",
                    Description = "Produces plate #A as well as copies of itself. Because of the high complexity of the product, the process takes longer than ordinary assembly. \nThe plate is vulnerable to background radiation and should be stored in a shielded container. \nIt is always a good idea to keep a few spares of Master plates.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ToolType = new ToolType() { ToolHandling = ToolHandlingType.Stationary, Durability = assemblerPlateDurability },
                    ItemType = new ItemType() { MaximumBulk = assemblerPlateBulk, },
                    NonLivingType = new NonLivingType()
                    {
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "nanoplate" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:assemblerPlateB")
                {
                    Name = "Assembler plate #B",
                    SummaryDescription = "Production plate for the molecular assembler",
                    Description = "Produces iron based products such as ammunition for the coil gun. \nLike all the assembler plates, it is vulnerable to background radiation and should be stored in a shielded container.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ToolType = new ToolType() { ToolHandling = ToolHandlingType.Stationary, Durability = assemblerPlateDurability },
                    ItemType = new ItemType() { MaximumBulk = assemblerPlateBulk, },
                    NonLivingType = new NonLivingType()
                    {
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "nanoplate" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:masterAssemblerPlateB")
                {
                    Name = "Master assembler plate #B",
                    SummaryDescription = "Replicating plate for the molecular assembler",
                    Description = "Produces plate #B as well as copies of itself. Because of the high complexity of the product, the process takes longer than ordinary assembly. \nThe plate is vulnerable to background radiation and should be stored in a shielded container. \nIt is always a good idea to keep a few spares of Master plates.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ToolType = new ToolType() { ToolHandling = ToolHandlingType.Stationary, Durability = assemblerPlateDurability },
                    ItemType = new ItemType() { MaximumBulk = assemblerPlateBulk, },
                    NonLivingType = new NonLivingType()
                    {
                        DegradeType = "equipment"
                    },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "nanoplate" } } }}
                });

                #endregion


                #endregion

                #region gadgets

                #region cloak
                listOfEntityTypes.Add(new EntityType("item:cloak")
                {
                    Name = "Cloak",
                    SummaryDescription = "Clothing fabric made of metamaterials that bend light.",
                    Description = "The cloak makes the wearer extremely hard to see. This makes it useful for hunting.",
                  
                    ItemType = new ItemType() 
                    { 
                        MaximumBulk = 0.07f, 
                        EffectsWhenEquipped = new[]{"cloaking"},
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.None },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Best }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.None } ,
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None }, 
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "equipment",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tarp" } } } }
                });
                #endregion

                #region night vision
                listOfEntityTypes.Add(new EntityType("item:nightVisionGoggles")
                {
                    Name = "Night vision goggles",
                    SummaryDescription = "Goggles that improve night vision.",
                    Description = "Visible light and infrared radiation is amplified, giving the wearer the ability to see at night.",

                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.05f,
                        EffectsWhenEquipped = new[] { "nightVision" },
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.None },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.None } ,
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.None }, 
                             { ItemType.TaskType.NightActivities, ItemType.AppropriateLevel.Best }
                        }
                    },                   
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment",
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    CategoryKey = "equipment",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } }
                });
                #endregion

             #region ground scanner
                listOfEntityTypes.Add(new EntityType("item:groundScanner")
                {
                    Name = "Ground scanner",
                    SummaryDescription = "Device that aids in detecting hidden resources and objects",
                    Description = "A sensor suite analyzes the ground surface to show its composition and reveal any objects of interest",

                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.05f,
                        EffectsWhenEquipped = new[] { "groundScanner" },
                        UseGearAtAnyDistanceFromExpedition = true,
                        TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {                            
                             { ItemType.TaskType.Examining, ItemType.AppropriateLevel.Best }                            
                        }
                    },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment",
                        
                    },
                   /* PartKeys = new SerializableDictionary<string,int>()
                    {
                        { "item:groundScannerPart", 1 }
                    },*/
                    CategoryKey = "equipment",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } }
                });


              /*  listOfEntityTypes.Add(new EntityType("item:groundScannerPart")
                {
                    Name = "Ground scanner part",
                   
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.05f                       
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment",
                    },
                    Category = GameData.Instance.AllItemCategories["equipment"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "greyPouch" } } } }
                });*/


                #endregion
            


                #endregion

                #region STRUCTURE ITEMS and PARTS

                listOfEntityTypes.Add(new EntityType("item:spikeTrap")
                {
                    Name = "Spring trap (item)",
                    SummaryDescription = "Must be set up using the BUILD button",
                    Description = "4 clenching iron spikes held back by a powerful spring. Set off by a trigger plate in the middle. This is a robust design which will kill smaller animals and maim bigger ones.", 
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    TierOrArea = new TierOrArea() { Tier = "basic" },//  for sec and for hunting food 
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = true,
                        DegradeType = "equipment",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:wroughtIron", 1 } }
                        //SalvageProcess = "salvageItemSpikeTrap"
                    },
                    CategoryKey = "rawMaterials",                    
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "metalSpikeTrapItem" } } } } //the sprite in the item billboard folder
                });

                listOfEntityTypes.Add(new EntityType("item:sensor")
                {
                    Name = "Motion sensor (item)",
                    SummaryDescription = "The sensor needs to be set up as a structure",
                    Description = "Once set up, the sensor will monitor its surroundings, allowing us to keep an eye on that area. Developed for the Tau Ceti mission. It is primarily powered by solar cells but will function day and night.",
                    ItemType = new ItemType() { MaximumBulk = 0.07f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = true,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "sensorItem" } } }}
                    //      Parts = new Dictionary<EntityType, int>() { { CreatePlaceholder("item:powerCell"), 1} }  //MP may 29 '14: We don't use battery because it's not supported and there's no workaround for avoiding the exploit of salvaging/rebuilding, where the battery then becomes magically recharged
                });


                listOfEntityTypes.Add(new EntityType("item:fieldLabPacked")
                {
                    Name = "Field lab (item)",
                    SummaryDescription = "The field lab must be set up using the BUILD button",
                    Description = "The field lab packed down for transport. Deploying the field lab makes us able to analyze samples and synthesize new drugs.\n It's being suggested that we instead salvage it and use its components for building a chemical liquid weapon.",
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Food },
                    ItemType = new ItemType() { MaximumBulk = 0.8f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = false,
                        DegradeType = "equipment",
                        SalvageProcess = "salvageFieldLabPacked",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:labComponents", 1 } }
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }                   
                });


                listOfEntityTypes.Add(new EntityType("item:labComponents")
                {
                    Name = "Lab components",
                    SummaryDescription = "Pump, tubes and small containers taken from the field lab",
                    Description = "These parts can be used for enhancing a fire extinguisher, making it able to project chemical liquid to defend against quadites.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" }, 
                    ItemType = new ItemType() { MaximumBulk = 0.6f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = false,
                        DegradeType = "equipment",

                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "labComponents" } } } }
                });

                listOfEntityTypes.Add(new EntityType("item:sentryWeaponMount")
                {
                    Name = "Sentry weapon mount",
                    SummaryDescription = "Rotating, automatically-aimed gun mount that must be outfitted with a weapon",
                    Description = "With some engineering skill and suitable components, a weapon can be attached here.",
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Security },
                    ItemType = new ItemType() { MaximumBulk = 0.6f, },

                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = false,
                        DegradeType = "equipment",

                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } } }
                });
                /* replaced with generic "item:gunBarrelSmoothShort"
                                listOfEntityTypes.Add(new EntityType("item:shotgunBarrel")
                                {
                                    Name = "Shotgun barrel",
                                    SummaryDescription = "A shotgun with some components dismantled",
                                    Description = "The stock and fore-end have been removed. What is left can be useful for making a new firearm",
                                    ItemType = new ItemType() { MaximumBulk = 0.05f, },
                                    NonLivingType = new NonLivingType()
                                    {
                                        Repairability = 1f,
                                        PartsAreWeatherProof = false,
                                        DegradeType = "equipment",

                                    },
                                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } } }
                                });
                */

                listOfEntityTypes.Add(new EntityType("item:structurePanels")
                {
                    Name = "Structure panels",
                    SummaryDescription = "Light weight, insulating, synthetic panels for simple, durable structures",
                    Description = "Versatile building material which can be made into many types of structures using simple tools.",//
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType() { MaximumBulk = 0.6f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = false,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "scrapMetal" } } } } //todo
                });

                listOfEntityTypes.Add(new EntityType("item:fieldKitchenStove")
                {
                    Name = "Field kitchen stove",
                    SummaryDescription = "The field kitchen needs to be set up using the BUILD button",
                    Description = "The stove for a field kitchen. Deploying the field kitchen creates a place for preparing food in relative comfort.",
                    ItemType = new ItemType() { MaximumBulk = 0.35f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Food },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = false,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }}
                });
                listOfEntityTypes.Add(new EntityType("item:fieldKitchenEquipment")
                {
                    Name = "Field kitchen equipment",
                    SummaryDescription = "The field kitchen needs to be set up using the BUILD button",
                    Description = "Equipment and structure parts for a field kitchen. Deploying the field kitchen creates a place for preparing food in relative comfort.",
                    ItemType = new ItemType() { MaximumBulk = 0.7f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced", Area = RatingTypes.Food },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = false,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:metalRefineryPart1")
                {
                    Name = "Refinery tanks",
                    SummaryDescription = "Large metal tanks which are components for the Refinery structure",
                    Description = "The tanks are part of the refinery installation which can separate rare-earth metals. They will be containing the chemical mixtures used in the processes. The refinery is highly compact and can be dismantled, moved and set up as needed.",
                    ItemType = new ItemType() { MaximumBulk = 0.35f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = false,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
                listOfEntityTypes.Add(new EntityType("item:metalRefineryEquipment")
                {
                    Name = "Refinery frame",
                    SummaryDescription = "The frame for the Refinery structure",
                    Description = "This component is part of the refinery installation which can separate rare-earth metals. The refinery is highly compact and can be dismantled, moved and set up as needed.",
                    ItemType = new ItemType() { MaximumBulk = 0.7f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = false,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
                listOfEntityTypes.Add(new EntityType("item:weatherStationMast")
                {
                    Name = "Weather station mast",
                    SummaryDescription = "The mast for various weather station sensors",
                    Description = "When deployed, the weather station will gather accurate data about the current weather. This can be used for making forecasts.",
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    ItemType = new ItemType() { MaximumBulk = 0.85f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        PartsAreWeatherProof = true,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:radio")
                {
                    Name = "Radio",
                    SummaryDescription = "Parts of a radio station which is used to communicate with other settlements", //
                    Description = "Most settlements maintain communication through radio when satellite communication is unavailable. If another radio station is within reach, they can be contacted in order to arrange trade deals.",
                    ItemType = new ItemType() { MaximumBulk = 0.18f, },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } } //todo
                });

                listOfEntityTypes.Add(new EntityType("item:radioAntenna")
                {
                    Name = "Radio antenna",
                    SummaryDescription = "Antenna for a small radio station",
                    Description = "Most settlements maintain communication through radio when satellite communication is unavailable. If another radio station is within reach, they can be contacted in order to arrange trade deals.",
                    ItemType = new ItemType() { MaximumBulk = 0.7f, },
                    TierOrArea = new TierOrArea() { Tier = "basic" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } } //todo
                });

                listOfEntityTypes.Add(new EntityType("item:weatherStationSensors")
                {
                    Name = "Weather station sensors",
                    SummaryDescription = "Sensors for monitoring the weather",
                    Description = "When deployed, the weather station will gather accurate data about the current weather. This can be used for making forecasts.",
                    ItemType = new ItemType() { MaximumBulk = 0.5f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:satelliteGroundStation")
                {
                    Name = "Satellite ground station",
                    SummaryDescription = "Enables satellite communication",
                    Description = "When deployed, the satellite ground station will enable communication with other sites using the satellites in orbit.",
                    ItemType = new ItemType() { MaximumBulk = 0.5f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } }}
                });
                #region molecular assembler components

                listOfEntityTypes.Add(new EntityType("item:vacuumChamber")
                {
                    Name = "Vacuum chamber",
                    SummaryDescription = "The vacuum chamber for the molecular assembler",
                    Description = "This chamber perfectly isolates the product from the environment while it is being produced by the assembler.",
                    ItemType = new ItemType() { MaximumBulk = 0.5f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:assemblerCabinet")
                {
                    Name = "Assembler cabinet",
                    SummaryDescription = "The cabinet for the molecular assembler",
                    Description = "This cabinet houses the molecular assembler. It has a port for materials.",
                    ItemType = new ItemType() { MaximumBulk = 1f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:assemblerCooling")
                {
                    Name = "Assembler cooling system",
                    SummaryDescription = "The cooling system for the molecular assembler",
                    Description = "This is the cooling system for the molecular assembler, it is needed to remove the heat generated in the process.",
                    ItemType = new ItemType() { MaximumBulk = 0.5f, },
                    TierOrArea = new TierOrArea() { Tier = "advanced" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } }}
                });
                #endregion


                #endregion

                #region decagonSeeds //not used
                /*          listOfEntityTypes.Add(new EntityType("item:decagonSeeds")
                {
                    Name = "Decagon seeds",  //for blow gun
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.02f,

                        

                        AmmunitionType = new AmmunitionType() { MaxNoOfRounds = 20 }
                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    Category = GameData.Instance.AllItemCategories["ammunition"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "guns" } } }}
                });*/
                #endregion
                #region none //NOT USED
                listOfEntityTypes.Add(new EntityType("item:none") //TODO: make it possible to make tool use optional
                {
                    Name = "none",
                    ItemType = new ItemType() { MaximumBulk = 0.3f, Abbreviation = "Too" },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    ToolType = new ToolType() { ToolHandling = ToolHandlingType.HandTool, Durability = 1 },
                    CategoryKey = "tools",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
                #endregion
                #region OLD TOOLS that ARE NOT CURRENTLY used - check if their stats are correct..

                /*               listOfEntityTypes.Add(new EntityType("item:electricalSaw")
                {
                    Name = "Electrical saw",
                    ToolType = new ToolType() { Durability = 0.9f, ToolHandling = ToolHandlingType.Stationary, ToolTag = new[] { "shapenSmallWood" } },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    Category = GameData.Instance.AllItemCategories["tools"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "powertool" } } }}
                });
                listOfEntityTypes.Add(new EntityType("item:handSaw")
                {
                    Name = "Hand saw",
                    ToolType = new ToolType() { Durability = 0.9f, ToolHandling = ToolHandlingType.HandTool, ToolTag = new[] { "shapenSmallWood" } },
                    ItemType = new ItemType() { MaximumBulk = 0.1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    Category = GameData.Instance.AllItemCategories["tools"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "equipment" } } }}
                });

                listOfEntityTypes.Add(new EntityType("item:chainsaw")
                {
                    Name = "Chainsaw",
                    ToolType = new ToolType() { ToolTag = new[] { "cutTurnipShell" }, ToolHandling = ToolHandlingType.HandTool, Durability = toolDurabilityAverage },
                    ItemType = new ItemType() { MaximumBulk = 0.3f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    Category = GameData.Instance.AllItemCategories["tools"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "powertool" } } }}
                });

*/
                #endregion

                #region Skimmer parts (flying, not wreck)
                 
                listOfEntityTypes.Add(new EntityType("item:skimmerMotor")
                {
                    Name = "Skimmer motor",
                    ItemType = new ItemType() { MaximumBulk = 1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
                listOfEntityTypes.Add(new EntityType("item:skimmerRotor")
                {
                    Name = "Rotor",
                    ItemType = new ItemType() { MaximumBulk = 1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                        
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
                listOfEntityTypes.Add(new EntityType("item:skimmerSeat")
                {
                    Name = "Seat",
                    ItemType = new ItemType() { MaximumBulk = 1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
                listOfEntityTypes.Add(new EntityType("item:skimmerCanopy")
                {
                    Name = "Canopy",
                    ItemType = new ItemType() { MaximumBulk = 1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
                listOfEntityTypes.Add(new EntityType("item:skimmerHull")
                {
                    Name = "Hull",
                    ItemType = new ItemType() { MaximumBulk = 6f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });

                listOfEntityTypes.Add(new EntityType("item:skimmerWing")
                {
                    Name = "Wing",
                    ItemType = new ItemType() { MaximumBulk = 4f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
                listOfEntityTypes.Add(new EntityType("item:skimmerLandingGear")
                {
                    Name = "Landing gear",
                    ItemType = new ItemType() { MaximumBulk = 4f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });

                #endregion

                #region Materials OLD //only used for Mule and some houses that are not in game.

                EntityType metalParts = new EntityType("item:metalParts")
                {
                    Name = "Metal parts",
                    ItemType = new ItemType() { Abbreviation = "Met", MaximumBulk = 0.1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "metal" } } } }
                };
                listOfEntityTypes.Add(metalParts);

                listOfEntityTypes.Add(new EntityType("item:cement")
                {
                    Name = "Cement",
                    ItemType = new ItemType() { Abbreviation = "Cem", MaximumBulk = 0.15f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "cement" } } } }
                });



                #endregion
                #region vehicle parts //

                listOfEntityTypes.Add(new EntityType("item:muleVehicleBody")
                {
                    Name = "Mule vehicle body",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } },
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 5f,

                    },
                    NonLivingType = new NonLivingType()
                    {
                        PartsAreWeatherProof = true,
                        Repairability = 0.8f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials"
                });

                listOfEntityTypes.Add(new EntityType("item:wheelMotor")
                {
                    Name = "Wheel motor",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } },
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.2f,

                    },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 0.8f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials"
                });
                listOfEntityTypes.Add(new EntityType("item:suspension")
                {
                    Name = "Suspension",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } },
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.2f,

                    },
                    NonLivingType = new NonLivingType()
                    {
                        PartsAreWeatherProof = true,
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials"
                });
                listOfEntityTypes.Add(new EntityType("item:vehicleSeat")
                {
                    Name = "Seat",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } },
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.6f,

                    },
                    NonLivingType = new NonLivingType()
                    {
                        PartsAreWeatherProof = true,
                        Repairability = 0.4f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials"
                });
                listOfEntityTypes.Add(new EntityType("item:controlPanel")
                {
                    Name = "Control panel",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } },
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.6f,

                    },
                    NonLivingType = new NonLivingType()
                    {
                        PartsAreWeatherProof = true,
                        Repairability = 0.4f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials"
                });
                listOfEntityTypes.Add(new EntityType("item:wheel")
                {
                    Name = "Wheel",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } },
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.4f,

                    },
                    NonLivingType = new NonLivingType()
                    {
                        PartsAreWeatherProof = true,
                        Repairability = 1f,
                        DegradeType = "equipment",
                        PartKeys = new SerializableDictionary<string, int>() { { "item:metalParts", 3 } }
                    },
                    CategoryKey = "rawMaterials"                   
                });
                listOfEntityTypes.Add(new EntityType("item:tyre")
                {
                    Name = "Tyre",
                    RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } },
                    ItemType = new ItemType()
                    {
                        MaximumBulk = 0.4f,

                    },
                    NonLivingType = new NonLivingType()
                    {
                        PartsAreWeatherProof = true,
                        Repairability = 0.3f,
                        DegradeType = "equipment"
                    },
                    CategoryKey = "rawMaterials"
                });

                #endregion

                #region Equipment not used

                /*  listOfEntityTypes.Add(new EntityType("item:testToolbox")
                {
                    Name = "Toolbox",
                    ItemType = new ItemType() { MaximumBulk = 1f, Abbreviation = "Adv", },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                     PartKeys = new SerializableDictionary<string,int>()
                     {
                         { "item:advancedKnife", 1}
                     },
                    Category = GameData.Instance.AllItemCategories["tools"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });*/
/*
                listOfEntityTypes.Add(new EntityType("item:basicTools")
                {
                    Name = "Basic tools",
                    ItemType = new ItemType() { MaximumBulk = 0.3f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment",
                    },
                    Category = GameData.Instance.AllItemCategories["tools"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
*/
/*
                listOfEntityTypes.Add(new EntityType("item:advancedTools")
                {
                    Name = "Advanced tools",
                    ItemType = new ItemType() { MaximumBulk = 1f },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    Category = GameData.Instance.AllItemCategories["tools"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "boxes" } } } }
                });
*/
/*
                listOfEntityTypes.Add(new EntityType("item:firstAidKit")
                {
                    Name = "First aid kit",
                    ItemType = new ItemType() { MaximumBulk = 1f, },
                    NonLivingType = new NonLivingType()
                    {
                        Repairability = 1f,
                        DegradeType = "equipment"
                    },
                    Category = GameData.Instance.AllItemCategories["rawMaterials"],
                    RenderableType = new RenderableType() { Default = new SpriteConditionInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "gadgets" } } } }
                });
*/

 


    
            
                





 









 
            #endregion


        }

        /// <summary>
        /// shared with/overridden by tut
        /// </summary>
        /// <param name="listOfEntityTypes"></param>
        /// <returns></returns>
        public static EntityType CreateDiamondKnife(List<EntityType> listOfEntityTypes)
        {
            EntityType knife = new EntityType("item:advancedKnife")
            {
                Name = "Knife (diamondoid carbon)",
                SummaryDescription = "All-purpose survival knife made from diamondoid carbon",
                Description = "The knife is made from carbon atoms arranged in a crystal structure similar to diamond, giving it exceptional hardness and durability. This is a product of a molecular assembler.",
                ToolType = new ToolType()
                {
                    Durability = toolDurabilityDurable,
                    ToolHandling = ToolHandlingType.HandTool,
                    ToolTag = new[] { "knife", "butcherFlesh", "cutThinShell" }
                },
                TierOrArea = new TierOrArea() { Tier = "advanced" },
                ItemType = new ItemType()
                {
                    HauledItemValue = ItemType.HauledItemValues.MostValuable,
                    MaximumBulk = 0.05f, //changed from 0.6f by AF on 22/5-14

                    AttachedObjectRenderableType = "knife",
                    AttachorTagToMountOn = "rightHand",
                    AttachesToBodyPart = "Right arm",
                    AnimStatesWhenAttached = new[] { AnimModifier.Knife },
                    WeaponType = new WeaponType()
                    {
                        AttackTypes = new AttackType[] { GameData.Instance.AllAttackTypes["knifeHack"] }
                        //DesirabilityForUseDefensive = 0.3f
                    },
                    TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>()
                        {
                             { ItemType.TaskType.LongerJourneys, ItemType.AppropriateLevel.Normal },
                             { ItemType.TaskType.UnspecifiedHunting, ItemType.AppropriateLevel.Minor },
                             { ItemType.TaskType.PatrolOrAttack, ItemType.AppropriateLevel.Minor } ,
                             { ItemType.TaskType.Scouting, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                             { ItemType.TaskType.Hauling, ItemType.AppropriateLevel.Minor }, //mp I'm setting this as minor/none for all...
                        }
                },
                NonLivingType = new NonLivingType()
                {
                    Repairability = 1f,
                    DegradeType = "equipment"
                },
                CategoryKey = "tools",
                RenderableType = new RenderableType() { DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "knife" } } } }

            };
            listOfEntityTypes.Add(knife);

            return knife;
            
            
        }
        
        
    }
    
}

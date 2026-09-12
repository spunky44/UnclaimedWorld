using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Collisions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.GatheringSites;
using UWGame.Client.Particles;

namespace UWGame.SimSide.AllGameData
{
    public class TreeLoader
    {

        public static void Init(List<EntityType> listOfEntityTypes)
        {
            string descriptionLivingSanctuaryTree = "\n BIOLOGY OVERVIEW\n The tree has a symbiotic relationship with the blue 'favorbread' which grows in close proximity. The tree will provide the favorbread with carbohydrates and in return, the favorbread collects mineral nutrients for the tree and seems to also protect the tree against diseases and pests.\n \n SURVIVAL GUIDE NOTES\n The favorbread is edible to humans without any preparation and is a cherished source of nutrition for anyone staying in the wilderness.";
            string descriptionDeadSanctuaryTree = "\n BIOLOGY OVERVIEW\n The tree has a symbiotic relationship with the blue 'favorbread' which grows in close proximity. The tree will provide the favorbread with carbohydrates and in return, the favorbread collects mineral nutrients for the tree and seems to also protect the tree against diseases and pests.\n \n SURVIVAL GUIDE NOTES\n The favorbread is edible and can be cultivated in a relatively simple way by excavating a pit next to a DEAD sanctuary tree. When we provide the favorbread with a source of carbohydrate, such as blackpulp, we can 'revive' the plant and make it grow fruits again.\n \n Note: This method of cultivation is not possible next to a LIVING sanctuary tree because disturbing the connection between the two organisms causes a dangerous, defensive response.";

            # region shadeleaf
            float bendyness = 0.8f;
            EntityType tree = new EntityType("tree:shadeleaf")
            {
                Name = "Shadeleaf",
                SummaryDescription = "Small deciduous tree that bends in the wind.",
                Description = "\n \n SURVIVAL GUIDE NOTES\n Though shadeleaf may be fit for smaller creatures attempting to shade themselves, its leaves are too thin and brittle to be of much use in crafting. However, the shadeleaf's thin, pliant limbs are particularly useful in small-scale shelter construction. Shadeleaf cane is also suitable when manufacturing arrow shafts, as its flexibility is ideal when accounting for accuracy.",
                ThumbnailSmall = "HUD_thumbnail_shadeleaf",
                TreeType = new TreeType()
                {                 
                   // Bendyness = 0.8f,
                    BulkPerSize = 4f,
                    MatureAge = 2f,
                    MaxAge = 12f,
                    SizeImpact = 0.6f,
                    MaxFlavours = 4,
                    FibrousPercentageOfTotalMass = 0f,
                    LumberPercentageOfFiberMass = 0f,
                    Crops = new string[]{ "crop:shadeleafCanes", "crop:shadeleafBowStave", "crop:sticks"},

                    DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:shadeleafCanes", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:shadeleafBowStave", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }

 
                         }
                },
                RenderableType = new RenderableType()
            {
                DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_shadeleaf_1_grown_summer" } } },
                 ClientStateConditions = new ClientStateInfo[]
                 {
                    // GROWN, WITH CANES:
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_1_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_2_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_3_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_4_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches)
                    },

                    // GROWN, WITH CANES, NIGHT:
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_1_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_2_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_3_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_4_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                    },

// GROWN, NO CANES:
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_1_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_2_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_3_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_4_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4)
                    },
// GROWN, NO CANES, NIGHT:
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_1_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_2_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_3_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_4_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Night)
                    },


                    // YOUNG
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_1_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_2_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                    },
                      new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_3_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_4_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young)
                    },

                    // YOUNG, NIGHT
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_1_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_2_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young, (int)StateModifier.Night)
                    },
                      new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_3_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleaf_4_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young, (int)StateModifier.Night)
                    },

                    // DEAD
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_1_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_2_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_1_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_2_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Dead)
                    },

                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_1_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_2_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_1_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_2_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young, (int)StateModifier.Dead)
                    },

                   // DEAD, NIGHT
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_1_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_2_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_1_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_2_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },

                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_1_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_2_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_1_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_2_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },



                 }

            },
                PointLayoutType = new PointLayoutType() { }
            };
                listOfEntityTypes.Add(tree);
            #endregion
                # region pale shadeleaf
                bendyness = 0.8f;
                tree = new EntityType("tree:paleshadeleaf")
                {
                    Name = "Pale shadeleaf",
                    SummaryDescription = "Small deciduous tree that bends in the wind.", //copied from ordinary shadeleaf
                    Description = "\n \n SURVIVAL GUIDE NOTES\n Though shadeleaf may be fit for smaller creatures attempting to shade themselves, its leaves are too thin and brittle to be of much use in crafting. However, the shadeleaf's thin, pliant limbs are particularly useful in small-scale shelter construction. Shadeleaf cane is also suitable when manufacturing arrow shafts, as its flexibility is ideal when accounting for accuracy.", 
                    ThumbnailSmall = "HUD_thumbnail_shadeleaf",
                    TreeType = new TreeType()
                    {
                        // Bendyness = 0.8f,
                        BulkPerSize = 4f,
                        MatureAge = 2f,
                        MaxAge = 12f,
                        SizeImpact = 0.6f,
                        MaxFlavours = 4,
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[] { "crop:shadeleafCanes", "crop:shadeleafBowStave", "crop:sticks" },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:shadeleafCanes", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:shadeleafBowStave", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }

 
                         }
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo() { RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_shadeleaf_1_grown_summer" } } },
                        ClientStateConditions = new ClientStateInfo[]
                 {
                    // GROWN, WITH CANES:
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_1_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_2_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_3_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_4_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches)
                    },

                    // GROWN, WITH CANES, NIGHT:
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_1_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_2_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_3_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_4_grown_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                    },

// GROWN, NO CANES:
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_1_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_2_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_3_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_4_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4)
                    },
// GROWN, NO CANES, NIGHT:
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_1_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_2_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_3_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_4_grown_cut", Bendyness = 0.1f }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Night)
                    },


                    // YOUNG
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_1_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_2_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                    },
                      new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_3_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_4_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young)
                    },

                    // YOUNG, NIGHT
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_1_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_2_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young, (int)StateModifier.Night)
                    },
                      new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_3_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young, (int)StateModifier.Night)
                    },
                    new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleaf_4_young_summer", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young, (int)StateModifier.Night)
                    },

                    // DEAD
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_1_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_2_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_1_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_2_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Dead)
                    },

                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_1_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_2_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_1_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young, (int)StateModifier.Dead)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_2_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young, (int)StateModifier.Dead)
                    },

                   // DEAD, NIGHT
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_1_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_2_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_1_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_2_grown", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },

                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_1_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_2_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_1_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },
                     new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_paleshadeleafdead_2_young", Bendyness = bendyness }},                                 
                        Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young, (int)StateModifier.Dead, (int)StateModifier.Night)
                    },



                 }

                    },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion

                #region shadeleaf dead //has very important resin
                // to be deleted when dead trees are implemented
                bendyness = 0.7f;
                tree = new EntityType("tree:shadeleafdead")
                {
                    Name = "Shadeleaf - dead",
                    SummaryDescription = "Dead shadeleaf tree. Supplies a useful resin.",
                    Description = "\n \n SURVIVAL GUIDE NOTES\n A beneficial resin can be found on dead shadeleaf trees: When attacked by scuttler bugs, the shadelaf tree produces and secretes a large amount of resin to ward off the bugs and protect other, nearby shadeleaf trees. We can harvest this resin and make use of its repellent effect by applying it to plant material thus protecting them against infestation. The resin also has adhesive properties and can be turned into a glue.",
                    TreeType = new TreeType()
                    {                      
                        BulkPerSize = 4f,
                        MatureAge = 2f,
                        MaxAge = 12f,
                        SizeImpact = 0.6f,
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{ "crop:sticks",
                               "crop:shadeleafResin"
                        },

                        DefaultCrops = new[]
                         {

                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:shadeleafResin", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                         }

                    },
                    RenderableType = new RenderableType()
                    { DefaultClientState = new ClientStateInfo()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_shadeleafdead_1_grown", Bendyness = bendyness } },
                        
                    },
                    ClientStateConditions = new ClientStateInfo[]
                        {                     
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_1_grown", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                        },
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_shadeleafdead_2_grown", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                        }}
                    },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);

#endregion

                # region daysheen
                bendyness = 0.6f;
                tree = new EntityType("tree:daysheen")
                {
                    Name = "Daysheen",
                    SummaryDescription = "Giant non-flowering plant with glossy, cone-shaped leaves.",
                    Description = "\n BIOLOGY OVERVIEW\n Daysheen is a massive clump-forming perennial with stiff, glossy leaves that form outward into a conical shape. Their glossy outer-coating may serve as a possible surface for glare reflection.\n \n SURVIVAL GUIDE NOTES\n The sheer size and structure of the daysheen's leaves, as well as their reflective capabilities, are substantial factors when considering shelter construction. ",
                    ThumbnailSmall = "HUD_thumbnail_daysheen",
                    TreeType = new TreeType()
                    {     
                        BulkPerSize = 16f,
                        MatureAge = 8f,
                        MaxAge = 20f,
                        MaxFlavours = 6,
                        SizeImpact = 2f, /*3f*/
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{ "crop:daysheenLeaves"                               
                        },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:daysheenLeaves", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 2 }
                  
                         }
                    },
                    GatheringSiteType = new GatheringSiteType()
                    {
                        arc = new Arc()
                        {
                            Radius = 5f,
                            MinAngle = -180,
                            MaxAngle = 180
                        },
                        SeatSize = 10,
                        MaxVisitors = 4

                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]
                                {new RenderAsBillboardType()
                                {
                                    AssetName = "tree_daysheen_1_grown_day"                                   
                                }
                                }
                        },
                         ClientStateConditions = new ClientStateInfo[]
                         {
                             // day: 2 grown sprites, 6 young sprites
                             // night: 3 grown sprites, 2 young sprites

                             // GROWN DAY
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.HasBranches)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.HasBranches)
                             },
                             // GROWN, NIGHT
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                             },
                             //GROWN CUT
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_cut", Bendyness = 0.2f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_cut", Bendyness = 0.1f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_cut", Bendyness = 0.2f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_cut", Bendyness = 0.1f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_cut", Bendyness = 0.2f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_cut", Bendyness = 0.1f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6)
                             },

                            //GROWN CUT, NIGHT
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_cut", Bendyness = 0.2f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_cut", Bendyness = 0.1f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_cut", Bendyness = 0.2f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_cut", Bendyness = 0.1f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_grown_cut", Bendyness = 0.2f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_grown_cut", Bendyness = 0.1f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Night)
                             },

                             // YOUNG DAY
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_young_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_young_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_3_young_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                             },
                              new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_4_young_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_5_young_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Young)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_6_young_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Young)
                             },

                             // YOUNG NIGHT
                              new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_young_night", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_young_night", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_1_young_night", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_young_night", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young, (int)StateModifier.Night)
                             }, 
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_young_night", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Young, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheen_2_young_night", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Young, (int)StateModifier.Night)
                             },



                             // DEAD
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheendead_1_grown", Bendyness = 0.3f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheendead_1_young", Bendyness = 0.3f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead, (int)StateModifier.Young)
                             },
                             // DEAD NIGHT
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheendead_1_grown", Bendyness = 0.3f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead, (int)StateModifier.Night)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheendead_1_young", Bendyness = 0.3f }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Dead, (int)StateModifier.Young, (int)StateModifier.Night)
                             }

                         }
                    },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 5)
                                {
                                    Offset = new Vector2(-10,-20)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);

                #endregion
                #region daysheen dead
                bendyness = 0.6f;
                tree = new EntityType("tree:daysheendead")
                {
                    Name = "Daysheen - dead",
                    SummaryDescription = "Giant non-flowering plant with glossy, cone-shaped leaves.",
                    Description = "\n BIOLOGY OVERVIEW\n Daysheen is a massive clump-forming perennial with stiff, glossy leaves that form outward into a conical shape. Their glossy outer-coating may serve as a possible surface for glare reflection.\n \n SURVIVAL GUIDE NOTES\n No use has been found for the dead daysheen.",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 16f,
                        MatureAge = 8f,
                        MaxAge = 20f,
                        SizeImpact = 2f, /*3f*/
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_daysheendead_1_grown", Bendyness = bendyness } },

                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                     
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheendead_1_grown", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                        },
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_daysheendead_1_young", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                        }}
                },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 5)
                                {
                                    Offset = new Vector2(-10,-20)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region Giant hollow
                bendyness = 0.1f;
                tree = new EntityType("tree:gianthollow")
                {
                    Name = "Giant hollow",  // maybe: http://en.wikipedia.org/wiki/Artichoke
                    SummaryDescription = "Hollow plant formed from large triangular scales",
                    Description = "\n BIOLOGY OVERVIEW\n These massive cavernous plants are native to arid, sunny landscapes. The giant hollows are home to many small animals, which offer the plant protection from predators in exchange for shelter and shade. The plant is able to uptake unusually large amounts of sulfate and thrives in areas with high sulfur concentrations.\n \n SURVIVAL GUIDE NOTES\n The buds of this plant are tough and have the size of a bowl. They could find use as an improvised food container.",
                    ThumbnailSmall = "HUD_thumbnail_greatHollow",
                    TreeType = new TreeType() { BulkPerSize = 40f, MatureAge = 22f, MaxAge = 400f, SizeImpact = 5f, FibrousPercentageOfTotalMass = 1f,
                        LumberPercentageOfFiberMass = 0f,
                        MaxFlavours = 3,
                        Crops = new string[]{                                
                                "crop:sticks",
                                "crop:giantHollowBud"
                        },

                        DefaultCrops = new[]
                         {
                            
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:giantHollowBud", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 } //mp dec 2014 would be nice if only on young (small) plants
                         }
                    }
                    ,
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_gianthollow_1_grown", Bendyness = bendyness } }
                        },
                        ClientStateConditions = new ClientStateInfo[]
                         {                            
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_gianthollow_1_grown", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_gianthollow_2_grown", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_gianthollow_3_grown", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                             },

                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_gianthollow_1_young", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_gianthollow_1_young", Bendyness = bendyness }},   // missing 2 young                              
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                             },
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_gianthollow_3_young", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                             },
                         }

                    },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 10)
                                {
                                    Offset = new Vector2(-10,-20)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);
                #endregion

                #region star snare
                bendyness = 0.1f;
                tree = new EntityType("tree:starSnare")
                {
                    Name = "Star snare",  // 
                    SummaryDescription = "Carnivorous plant that attracts flying prey at night by emitting light",
                    Description = "The tentacles crowning the top can lure and ensnare flyers such as diamond birds so they fall into the digestive fluid of the plant's hollow interior where they dissolve.",//todo
                    ThumbnailSmall = "HUD_thumbnail_starsnare",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 40f,
                        MatureAge = 22f,
                        MaxAge = 200f,
                        SizeImpact = 5f,
                        FibrousPercentageOfTotalMass = 1f,
                        LumberPercentageOfFiberMass = 0f,
                        MaxFlavours = 3,
            /*            Crops = new string[]{                                
                                
                                GameData.Instance.AllResourceTypes["crop:sticks"],
                                GameData.Instance.AllResourceTypes["crop:giantHollowBud"]
                        },*/

              /*          DefaultCrops = new[]
                         {
                            
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:giantHollowBud", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 } //
                         }*/
                    }
                    ,
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_starsnare_1_grown_day", Bendyness = bendyness } }
                        },
                        ClientStateConditions = new ClientStateInfo[]
                         {                            
                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_starsnare_1_grown_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                             },
        

                             new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_starsnare_1_young_day", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                             },

                              new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_starsnare_1_grown_night", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Night)
                             }, 

                            new ClientStateInfo()
                             {
                                  RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_starsnare_1_young_night", Bendyness = bendyness }},                                 
                                  Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Night)
                             },

                         }

                    },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 15)
                                {
                                    Offset = new Vector2(0, -8)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);

                #endregion


                #region brambles
                bendyness = 0.2f;
                tree = new EntityType("tree:brambletiny")
                {
                    Name = "Iron bramble - small",
                    SummaryDescription = "Tangled bushes that form a dense barrier.",
                    Description = "\n BIOLOGY OVERVIEW\n Iron bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.", // Phantom weaver is one of the creatures that nests here
                    ThumbnailSmall = "HUD_thumbnail_bramble",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
           /*         Crops = new string[]{   "crop:thorns"    },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:thorns", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                  
                         }*/
                    },

                    GatheringSiteType = new GatheringSiteType()
                    {
                        arc = new Arc()
                        {
                            Radius = 8f,
                            MinAngle = -180,
                            MaxAngle = 180
                        },
                        SeatSize = 1,
                        MaxVisitors = 2

                    },

                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_brambletiny_1_grown", Bendyness = bendyness } },

                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                     
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_brambletiny_1_young", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Young)
                        }}
                    },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 14)
                                {
                                    Offset = new Vector2(-1,-4)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);

                /*tree = new EntityType("tree:bramble")
                {
                    Name = "Bramble",
                    TreeType = new TreeType()
                    {
                        SpriteName = "bramble",
                        Bendyness = 0.2f,
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                {new RenderAsBillboardType()
                {
                   /* SpriteName = "daysheen"*/
                /*}
                }
                    },
                    DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 17)
                            {
                                Offset = new Vector2(-1,-11)
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);*/

                bendyness = 0.2f;
                tree = new EntityType("tree:bramblesmall")
                {
                    Name = "Iron bramble - larger",
                    SummaryDescription = "Tangled bushes that form a dense barrier.",
                    Description = "\n BIOLOGY OVERVIEW\n Iron bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.",
                    TreeType = new TreeType()
                    {                      
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
           /*       Crops = new string[]{   "crop:thorns"    },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:thorns", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                  
                         }*/
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_bramblesmall_1_grown", Bendyness = bendyness } },

                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                     
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_bramblesmall_1_young", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Young)
                        }}
                    },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 19)
                                {
                                    Offset = new Vector2(0,-8)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);

                bendyness = 0.2f;
                tree = new EntityType("tree:bramblemedium")
                {
                    Name = "Iron bramble - big",
                    SummaryDescription = "Tangled bushes that form a dense barrier.",
                    Description = "\n BIOLOGY OVERVIEW\n Iron bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                 /*    Crops = new string[]{  "crop:thorns"       },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:thorns", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 1 }
                  
                         }*/
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_bramblemedium_1_grown", Bendyness = bendyness } },

                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                     
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_bramblemedium_1_young", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Young)
                        }}
                    },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 23)
                                {
                                    Offset = new Vector2(0,-16)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);
            #endregion
                #region Spoak
                bendyness = 1.5f;
                tree = new EntityType("tree:spoak")
                {
                    Name = "Spoak",  //Spoak
                    SummaryDescription = "Hardwood tree with stiff leaves.",
                    Description = "\n BIOLOGY OVERVIEW\n The wood's properties and thick trunk that spreads into spiraling branches led us to the name 'spiral oak', later condensed to the portmanteau: 'spoak'. Its leaves are large, rigid plates arranged in a pattern that allows for maximal solar energy to be collected, while at the same time letting strong winds pass through the canopy without toppling the tree.\n \n SURVIVAL GUIDE NOTES\n Both leaves and branches are useful in construction.", 
                    ThumbnailSmall = "HUD_thumbnail_spoak",
                    TreeType = new TreeType()
                    {                        
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        MaxFlavours = 5,
                        FibrousPercentageOfTotalMass = 1f,
                        LumberPercentageOfFiberMass = 0.5f,
                        Crops = new string[]{                                 
                                 "crop:spoakBranches",
                                 "crop:sticks"
                        },
                        DefaultCrops = new[]
                         {
                             
                             new DefaultCrops(){ KeyName = "crop:spoakBranches", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }, //mp wish i could go below 0 for minimumitems..

                         }
                    },
                   
                    GatheringSiteType = new GatheringSiteType()
                    {
                        arc = new Arc()
                        {
                            Radius = 5f,
                            MinAngle = -180,
                            MaxAngle = 180
                        },
                        SeatSize = 10,
                        MaxVisitors = 4

                    },

                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_holdenstree_1_grown_summer_nofruit", Bendyness = bendyness, } },

                        },

                        
                        ClientStateConditions = new ClientStateInfo[]
                        { 
                            // YOUNG
                            new ClientStateInfo()
                            {                                 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                                   
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            }, 
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_3_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_4_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_5_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Young)
                            },

                            // GROWN, NO FRUIT
                         /*   new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_1_grown_summer_nofruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour1)
                            },
                            new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_2_grown_summer_nofruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour2)
                            },
                             new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_3_grown_summer_nofruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour3)
                            },
                             new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_4_grown_summer_nofruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour4)
                            },
                             new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_5_grown_summer_nofruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour5)
                            },*/


/////////////////////GROWN, WITH BRANCHES:
                             new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_1_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_2_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_3_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_4_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_5_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.HasBranches)
                            },

/////////////////////GROWN, WITH BRANCHES, NIGHT:
                             new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_1_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_2_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_3_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_4_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                ParticleEmitters = new ParticleEmitterEffect[]{  new ParticleEmitterEffect() { ParticleSystemKey = "pollen" } }, 
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_5_grown_summer_nofruit", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },

                       //GROWN, CUT                       
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_1_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_2_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_3_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_4_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_5_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5)
                            },

                       //GROWN, CUT, NIGHT                       
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_1_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_2_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_3_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_4_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_5_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Night)
                            },



                            // GROWN, UNRIPE FRUIT
                          /*  new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_1_grown_summer_unripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour1, (int)SpriteModifier.HasResources)
                            },
                            new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_2_grown_summer_unripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour2, (int)SpriteModifier.HasResources)
                            },
                            new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_3_grown_summer_unripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour3, (int)SpriteModifier.HasResources)
                            },
                            new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_4_grown_summer_unripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour4, (int)SpriteModifier.HasResources)
                            },
                            new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_5_grown_summer_unripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour5, (int)SpriteModifier.HasResources)
                            },*/

                            // GROWN, RIPE FRUIT
                           /*  new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_1_grown_summer_ripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour1, (int)SpriteModifier.HasResources, (int)SpriteModifier.Ripe)
                            },
                            new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_2_grown_summer_ripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour2, (int)SpriteModifier.HasResources, (int)SpriteModifier.Ripe)
                            },
                            new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_3_grown_summer_ripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour3, (int)SpriteModifier.HasResources, (int)SpriteModifier.Ripe)
                            },
                            new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_4_grown_summer_ripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour4, (int)SpriteModifier.HasResources, (int)SpriteModifier.Ripe)
                            },
                            new SpriteConditionInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_holdenstree_5_grown_summer_ripefruit" }},                                 
                                Conditions = new BitMask64(typeof(SpriteModifier), (int)SpriteModifier.Flavour5, (int)SpriteModifier.HasResources, (int)SpriteModifier.Ripe)
                            },*/
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region spoak: dead leaves, naked, vines
                bendyness = 1.5f;
                tree = new EntityType("tree:spoakdeadleaves")
                {
                    Name = "Spoak - dead leaves",
                    SummaryDescription = "A dying spoak tree.",
                    Description = "\n BIOLOGY OVERVIEW\n The wood's properties and the thick trunk that spreads into spiralling branches made us name this tree 'spiral oak', later contracted to the portmanteau 'spoak'. Its leaves are large, rigid plates that seem to be arranged in a pattern that allows maximum solar energy to be collected while at the same time letting strong winds pass through the canopy without toppling the tree.", 
                    TreeType = new TreeType()
                    {
                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{                                 
                                "crop:spoakBranches",
                                "crop:sticks"
                        },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:spoakBranches", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                         }
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_holdenstreedeadleaves_1_grown", Bendyness = bendyness } }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);

                bendyness = 0.2f;
                tree = new EntityType("tree:spoakdeadnaked")
                {
                    Name = "Spoak - dead, naked",
                    SummaryDescription = "A dead spoak tree.",
                    Description = "\n BIOLOGY OVERVIEW\n The wood's properties and the thick trunk that spreads into spiralling branches made us name this tree 'spiral oak', later contracted to the portmanteau 'spoak'.", 
                    TreeType = new TreeType()
                    {                      
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{                                 
                                "crop:spoakBranches",
                                "crop:sticks"
                        },

                        DefaultCrops = new[]
                         {
                             
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                         }
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_holdenstreedeadnaked_1_grown", Bendyness = bendyness } }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);

                bendyness = 0.4f;
                tree = new EntityType("tree:spoakdeadvines")
                {
                    Name = "Spoak - dead, overgrown",
                    SummaryDescription = "A dead spoak tree, overgrown with weeds.",
                    Description = "\n BIOLOGY OVERVIEW\n The wood's properties and the thick trunk that spreads into spiralling branches made us name this tree 'spiral oak', later contracted to the portmanteau 'spoak'.", 
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                     Crops = new string[]{                                 
                            //    GameData.Instance.AllResourceTypes["crop:vine"], // mp april 2015. made into ground resource. we have to choose, else there's duplication on the gather window.
                                "crop:spoakBranches",
                                "crop:sticks"
                        },
                     
                        DefaultCrops = new[]
                         {
                          //   new DefaultCrops(){ KeyName = "crop:vine", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 2 }, 
                             new DefaultCrops(){ KeyName = "crop:spoakBranches", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                         }
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_holdenstreedeadvines_1_grown", Bendyness = bendyness } }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion

                #region copperfern
                bendyness = 0.5f;
                listOfEntityTypes.Add(new EntityType("tree:copperfern")
                {
                    Name = "Copperfern",
                    SummaryDescription = "A bush often found on sandy soil",
                    Description = "\n BIOLOGY OVERVIEW\n Though the copperfern's leaves take on the appearance of those of a fern, the plant more closely resembles a shrub. Microscopic aphid-like insects inhabit the bush, releasing a sweet, irresistible substance similar to honeydew. This substance attracts various other insects for feeding and nesting, including the pig fly.", 
                    ThumbnailSmall = "HUD_thumbnail_coppperfern",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                        MaxFlavours = 3,
                         Crops = new string[]{ "crop:pigFlies" },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:pigFlies", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 2 } // increased the value since there's a very low amount of copperfern on twinkler island
                           
                         }
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_copperfern_1_grown_summer", Bendyness = bendyness } }
                        }, 
                        ClientStateConditions = new ClientStateInfo[]
                        { 
                            // 3 grown, 2 young sprites
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_copperfern_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_copperfern_2_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_copperfern_3_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                            },

                            // YOUNG
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_copperfern_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_copperfern_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_copperfern_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                            },
                            // DEAD
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_copperferndead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_copperferndead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_copperferndead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead)
                            }

                        }

                    },
                    PointLayoutType = new PointLayoutType() { }
                });
                #endregion
                #region copperfern dead
                bendyness = 0.4f;
                listOfEntityTypes.Add(new EntityType("tree:copperferndead")
                {
                    Name = "Copperfern - dead",
                    SummaryDescription = "A bush often found on sandy soil",
                    Description = "\n BIOLOGY OVERVIEW\n Though the copperfern's leaves take on the appearance of those of a fern, the plant more closely resembles a shrub. Microscopic aphid-like insects inhabit the bush, releasing a sweet, irresistible substance similar to honeydew. This substance attracts various other insects for feeding and nesting, including the pig fly.", 
                    TreeType = new TreeType()
                    {                      
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_copperferndead_1_grown", Bendyness = bendyness } }
                        }
                    },
                    PointLayoutType = new PointLayoutType() { }
                });
                #endregion
                #region  pale copperfern
                bendyness = 0.5f;
                listOfEntityTypes.Add(new EntityType("tree:palecopperfern")
                {
                    Name = "Pale copperfern", //text copied from ordinary copperfern
                    SummaryDescription = "A bush often found on sandy soil",
                    Description = "\n BIOLOGY OVERVIEW\n Though the copperfern's leaves take on the appearance of those of a fern, the plant more closely resembles a shrub. Microscopic aphid-like insects inhabit the bush, releasing a sweet, irresistible substance similar to honeydew. This substance attracts various other insects for feeding and nesting, including the pig fly.",
                    ThumbnailSmall = "HUD_thumbnail_coppperfern",
                    TreeType = new TreeType()
                    {
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                        MaxFlavours = 3,
                        Crops = new string[] { "crop:pigFlies" },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:pigFlies", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 2 } // increased the value since there's a very low amount of copperfern on twinkler island
                           
                         }
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_palecopperfern_1_grown_summer", Bendyness = bendyness } }
                        },
                        ClientStateConditions = new ClientStateInfo[]
                        { 
                            // 3 grown, 2 young sprites
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palecopperfern_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palecopperfern_2_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palecopperfern_3_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                            },

                            // YOUNG
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palecopperfern_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palecopperfern_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palecopperfern_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                            },
                            // DEAD
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palecopperferndead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palecopperferndead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palecopperferndead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead)
                            }

                        }

                    },
                    PointLayoutType = new PointLayoutType() { }
                });
                #endregion
                #region pale copperfern dead
                bendyness = 0.4f;
                listOfEntityTypes.Add(new EntityType("tree:palecopperferndead")
                {
                    Name = "Pale copperfern - dead",//
                    SummaryDescription = "A bush often found on sandy soil",
                    Description = "\n BIOLOGY OVERVIEW\n Though the copperfern's leaves take on the appearance of those of a fern, the plant more closely resembles a shrub. Microscopic aphid-like insects inhabit the bush, releasing a sweet, irresistible substance similar to honeydew. This substance attracts various other insects for feeding and nesting, including the pig fly.",
                    TreeType = new TreeType()
                    {
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_palecopperferndead_1_grown", Bendyness = bendyness } }
                        }
                    },
                    PointLayoutType = new PointLayoutType() { }
                });
                #endregion
                #region Clearleaf tree     was..rhubarb
                bendyness = 0.2f;
                tree = new EntityType("tree:rhubarb")
                {
                    Name = "Clearleaf tree", //Glassleaf tree
                    SummaryDescription = "Odd plant whose leaves are shaped as transparent disks",
                    Description = "These knobby plant structures grow on top of muckroot - the grey/blue mossy carpet found in certain areas. Like other plants which grow on muckroot, the clearleaf is in symbiosis with the muckroot which provides it with nutrients.",
                    ThumbnailSmall = "HUD_thumbnail_rhubarb",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,                  
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        MaxFlavours = 3,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType() { 
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_rhubarb_1_grown_summer", Bendyness = bendyness } }
                        }, 
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {                                
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_rhubarb_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_rhubarb_2_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_rhubarb_3_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_rhubarb_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_rhubarb_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_rhubarb_3_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                            },

                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region desert clearleaf tree     was..rhubarb
                bendyness = 0.2f;
                tree = new EntityType("tree:desertrhubarb")
                {
                    Name = "Desert clearleaf tree",
                    SummaryDescription = "Odd plant whose leaves are shaped as transparent disks",
                    Description = "This variant of the clearleaf tree has adapted to grow in arid environments by evolving an ability to conserve water. There is still much research to be done before we understand the details.",
                    ThumbnailSmall = "HUD_thumbnail_rhubarb",
                    TreeType = new TreeType()
                    {
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        MaxFlavours = 3,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_desertrhubarb_1_grown_summer", Bendyness = bendyness } }
                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {                                
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_desertrhubarb_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_desertrhubarb_2_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_desertrhubarb_3_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_desertrhubarb_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_desertrhubarb_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_desertrhubarb_3_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                            },

                        }
                    },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region sanctuary tree
                tree = new EntityType("tree:sanctuarytree")
                {
                    Name = "Sanctuary tree",  
                    SummaryDescription = "Colossal tree that grows in the firegrass biome",
                    Description = descriptionLivingSanctuaryTree,
                    ThumbnailSmall = "HUD_thumbnail_sanctuarytree",
                    TreeType = new TreeType()
                    {                      
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        HasSummerWinterCycle = true,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{                                
                            "crop:sticks"
                        },

                        DefaultCrops = new[]
                         {
                            
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 2 }
                         }
                    },
                    RenderableType = new RenderableType() {  
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_sanctuarytree_1_grown_summer" } }
                        }, 
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_sanctuarytree_1_grown_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_sanctuarytree_1_young_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_sanctuarytree_1_grown_winter" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Winter)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_sanctuarytree_1_young_winter" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Winter)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_sanctuarytreedead_1_grown" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_sanctuarytreedeadvines_1_grown" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead, (int)StateModifier.Overgrown) // change flavour..?
                            }


                        }
                    },
                    //This is an example of how to apply a geo layout to a giant tree
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 45)
                                {
                                    Offset = new Vector2(-10,-20)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);
#endregion
                #region sanctuary tree dead + overgrown
                tree = new EntityType("tree:sanctuarytreedead")
                {
                    Name = "Sanctuary tree - dead",
                    SummaryDescription = "The remnants of a colossal tree that inhabits the firegrass biome",
                    Description = descriptionDeadSanctuaryTree,
                    ThumbnailSmall = "HUD_thumbnail_placeholder",
                    IsSelectable = true,
                    UsesMemory = true,
                    ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                    SharedSpecialActions = new[] { new Pair<string, bool>("buildFavorbreadFarm", true) },//
                    TreeType = new TreeType()
                    {                     
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{                                
                            "crop:sticks"
                        },

                        DefaultCrops = new[]
                         {
                            
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 2 }
                         }
                    },
                    RenderableType = new RenderableType() { 
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_sanctuarytreedead_1_grown" } }
                        }
                },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 45)
                                {
                                    Offset = new Vector2(-10,-20)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);

                tree = new EntityType("tree:sanctuarytreedeadvines")
                {
                    Name = "Sanctuary tree - dead", 
                    SummaryDescription = "The remnants of a colossal tree that inhabits the firegrass biome",
                    Description = descriptionDeadSanctuaryTree,
                    ThumbnailSmall = "HUD_thumbnail_placeholder",
                    IsSelectable = true,
                    UsesMemory = true,
                    ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
                    SharedSpecialActions = new[] { new Pair<string, bool>("buildFavorbreadFarm", true) },//
                    TreeType = new TreeType()
                    {                      
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{                                
                            "crop:sticks",
                            //    GameData.Instance.AllResourceTypes["crop:vine"] //mp is tile resource instead
                        },
 
                        DefaultCrops = new[]
                         {
                            
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 2 },
                        //     new DefaultCrops(){ KeyName = "crop:vine", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 3 }
                         }
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_sanctuarytreedeadvines_1_grown" } }
                        }
                },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 45)
                                {
                                    Offset = new Vector2(-10,-20)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);
                #endregion

                #region greentub
                tree = new EntityType("tree:greentub")
                {
                    Name = "Greentub",
                    SummaryDescription = "Cylindrical, spungy growths that thrive on muckroot",
                    Description = "These barrel shaped plants live on top of 'muckroot' - the blue crust of moss which covers rivers and streams. Like all plants that live there, the greentubs are symbiotes - they do not have roots and muckroot provides them with nutrients.",
                    ThumbnailSmall = "HUD_thumbnail_greentub",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        MaxFlavours = 3,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                 /*       Crops = new string[]{                                
                            
                                GameData.Instance.AllResourceTypes["crop:marshcotSap"]
                        },

                        DefaultCrops = new[]
                         {
                            
                             new DefaultCrops(){ KeyName = "crop:marshcotSap", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 2 }
                         }*/
                    },
                    RenderableType = new RenderableType() { 
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_greentub_1_grown_summer" } }
                        }, 
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_greentub_1_grown_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_greentub_2_grown_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_greentub_3_grown_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_greentub_1_young_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_greentub_2_young_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_greentub_3_young_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                            }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region goblin sprouts
                tree = new EntityType("tree:goblinsprouts")
                {
                    Name = "Goblin sprouts",
                    SummaryDescription = "Spherical, spungy growth. Ubiquitous on muckroot",
                    Description = "These are among the various plants that live on top of 'muckroot' - the blue crust of moss which covers rivers and streams. Like all plants that live there, the goblin sprouts are symbiotes - they do not have roots and muckroot provides them with nutrients.", // 
                    ThumbnailSmall = "HUD_thumbnail_goblinsprouts",
                    TreeType = new TreeType()
                    {                     
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        MaxFlavours = 2,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,

                    },

                    GatheringSiteType = new GatheringSiteType()
                    {
                        arc = new Arc()
                        {
                            Radius = 5f,
                            MinAngle = -180,
                            MaxAngle = 180
                        },
                        SeatSize = 5,
                        MaxVisitors = 2

                    },

                    RenderableType = new RenderableType() { 
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_goblinsprouts_1_grown_summer" } }
                        }, 
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_goblinsprouts_1_grown_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_goblinsprouts_2_grown_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_goblinsprouts_1_young_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_goblinsprouts_2_young_summer" }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            },
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region marshcot
                bendyness = 0.1f;
                tree = new EntityType("tree:marshcotflower")
                {
                    Name = "Marshcot flower",
                    SummaryDescription = "Giant flower with petals that resemble satin pillows",
                    Description = "\n NOTES\n We have found that the sap of the marshcot can be used for making rubber.\n \n BIOLOGY OVERVIEW\n The flower of the marshcot can reach the size of a car. The marshcot grows in brackish wetlands and swamps where its thick tangled stems can cover large areas above and under water.",
                    ThumbnailSmall = "HUD_thumbnail_marshcot",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        MaxFlavours = 2,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                                       Crops = new string[]{  "crop:marshcotSap"  },         
                                           DefaultCrops = new[]
                                            {
                            
                                                new DefaultCrops(){ KeyName = "crop:marshcotSap", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 2  } 
                                            }
                    },

                    GatheringSiteType = new GatheringSiteType()
                    {
                        arc = new Arc()
                        {
                            Radius = 5f,
                            MinAngle = -180,
                            MaxAngle = 180
                        },
                        SeatSize = 5,
                        MaxVisitors = 2

                    },

                    RenderableType = new RenderableType()
                    {
                       DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_marshcotflower_1_grown", Bendyness = bendyness } }
                        }, 
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotflower_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotflower_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotflower_1_young", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotflower_1_young", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            },                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotflowerdead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                            },                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotflowerdead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead)
                            }

                            
                        }
                    },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);

                bendyness = 0.1f;
                tree = new EntityType("tree:marshcotflowerdead")
                {
                    Name = "Marshcot flower - dead",
                    SummaryDescription = "The flower of a dead marshcot plant",
                    Description = "\n NOTES\n We have found that the sap of the marshcot can be used for making rubber.\n \n BIOLOGY OVERVIEW\n The flower of the marshcot can reach the size of a car. The marshcot grows in brackish wetlands and swamps where its thick tangled stems can cover large areas above and under water.",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType() {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_marshcotflowerdead_1_grown", Bendyness = bendyness } }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);

                bendyness = 0.4f;
                tree = new EntityType("tree:marshcotleaf")
                {
                    Name = "Marshcot leaf",
                    SummaryDescription = "Large, stiff leaf on the marshcot plant",
                    Description = "\n NOTES\n We have found that the sap of the marshcot can be used for making rubber.\n \n BIOLOGY OVERVIEW\n The marshcot grows in brackish wetlands and swamps where its thick tangled stems can cover large areas above and under water.",
                    ThumbnailSmall = "HUD_thumbnail_marshcot",
                    TreeType = new TreeType()
                    {                        
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        MaxFlavours = 3,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                                       Crops = new string[]{ "crop:marshcotSap"  },                       

                                           DefaultCrops = new[]
                                            {
                            
                                                new DefaultCrops(){ KeyName = "crop:marshcotSap", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 1  } 
                                            }

                    },
                    RenderableType = new RenderableType() { 
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_marshcotleaf_1_grown", Bendyness = bendyness } }
                        }, 
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotleaf_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotleaf_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotleaf_3_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotleafdead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotleafdead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead)
                            }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);

                bendyness = 0.3f;
                tree = new EntityType("tree:marshcotleafdead")
                {
                    Name = "Marshcot leaf - dead",
                    SummaryDescription = "Leaf on a dead marshcot plant",
                    Description = "\n NOTES\n We have found that the sap of the marshcot can be used for making rubber.\n \n BIOLOGY OVERVIEW\n The marshcot grows in brackish wetlands and swamps where its thick tangled stems can cover large areas above and under water.",
                    ThumbnailSmall = "HUD_thumbnail_marshcot",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        MaxFlavours = 2,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType() {  
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_marshcotleafdead_1_grown", Bendyness = bendyness } }
                        }, 
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotleafdead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_marshcotleafdead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region thistle tree

                bendyness = 0.4f;
                tree = new EntityType("tree:thistle")
                {
                    Name = "Thistle tree",
                    ThumbnailSmall = "HUD_thumbnail_thistle",
                    TreeType = new TreeType()
                    {                      
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_thistle_1_grown", Bendyness = bendyness } }
                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_thistle_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_thistle_1_young", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_thistledead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                            }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);

                bendyness = 0.4f;
                tree = new EntityType("tree:thistledead")
                {
                    Name = "Thistle tree - dead",
                    ThumbnailSmall = "HUD_thumbnail_thistle",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_thistledead_1_grown", Bendyness = bendyness } }
                        }
                    },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region geo stalk    ...was candy stalk
                bendyness = 0.7f;
                tree = new EntityType("tree:candystalk")
                {
                    Name = "Geo stalk",
                    SummaryDescription = "Strange, crystalline plant life that grows from rock crevices",
                    Description = "Based on its habitat, it seems that its crystalline structure is grown from an, as yet, unknown biological composition, derived from geode crystals which are high in minerals.",
                    ThumbnailSmall = "HUD_thumbnail_candystalk",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_candystalk_1_grown_summer", Bendyness = bendyness } }
                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                            
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_candystalk_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_candystalk_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region wingweed

                bendyness = 0.5f;
                tree = new EntityType("tree:wingweed")
                {
                    Name = "Wingweed",
                    SummaryDescription = "Herbaceous plant with large, soft leaves. Common on firegrass",
                    Description = "\n BIOLOGY OVERVIEW\n Wingweed often grows in temperate, drier climes, which makes its presence amongst firegrass almost a constant; however the plant has an excellent plasticity and can be found in almost any habitat. Due to their proximity to the ground, the species generally have larger leaves with which to absorb sunlight.\n \n SURVIVAL GUIDE NOTES\n The leaves can serve as a temporary insulation as they are quite proficient at retaining heat.",
                    ThumbnailSmall = "HUD_thumbnail_wingweed",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 1f,
                        MatureAge = 5f,
                        MaxAge = 10f,
                        SizeImpact = 0.6f,
                        MaxFlavours = 7,
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{                                 
                               "crop:wingweedLeaves"
                             
                        },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:wingweedLeaves", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1, }
                             
                         }
                    },

                    GatheringSiteType = new GatheringSiteType()
                    {
                        arc = new Arc()
                        {
                            Radius = 3f,
                            MinAngle = -180,
                            MaxAngle = 180
                        },
                        SeatSize = 5,
                        MaxVisitors = 2

                    },

                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_wingweed_1_grown_summer", Bendyness = bendyness } }
                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {     
                            // GROWN
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_2_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_3_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_4_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_5_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_6_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_7_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.HasBranches)
                            },

                            // GROWN, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_2_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_3_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_4_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_5_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_6_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_7_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },


                            // GROWN, NO LEAVES
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_1_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_2_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_3_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_4_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_5_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_6_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_7_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7)
                            },

                            // GROWN, NO LEAVES, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_1_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_2_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_3_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_4_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_5_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_6_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_7_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Night)
                            },



                            // YOUNG
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_3_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_4_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_5_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_6_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_7_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Young)
                            },

                            // YOUNG, NIGHT
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_3_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_4_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_5_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_6_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweed_7_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Young, (int)StateModifier.Night)
                            },

                            // DEAD
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Dead)
                            },

                           // DEAD, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },

                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_wingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Dead, (int)StateModifier.Night)
                            }

                        }
                },
                    PointLayoutType = new PointLayoutType() { IsBlocking = false }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region desert wingweed

                bendyness = 0.5f;
                tree = new EntityType("tree:desertWingweed")
                {
                    Name = "Desert wingweed",
                    SummaryDescription = "Herbaceous plant with large, soft leaves. This variant is common on dry soil",//
                    Description = "\n BIOLOGY OVERVIEW\n Wingweed often grows in temperate, drier climes, which makes its presence amongst firegrass almost a constant; however the plant has an excellent plasticity and the desert variant can be found on very dry soil. Due to their proximity to the ground, the species generally have larger leaves with which to absorb sunlight.\n \n SURVIVAL GUIDE NOTES\n The leaves can serve as a temporary insulation as they are quite proficient at retaining heat.",//TODO
                    ThumbnailSmall = "HUD_thumbnail_wingweed",
                    TreeType = new TreeType()
                    {
                        BulkPerSize = 1f,
                        MatureAge = 5f,
                        MaxAge = 10f,
                        SizeImpact = 0.6f,
                        MaxFlavours = 7,
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{                                 
                               "crop:wingweedLeaves"
                             
                        },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:wingweedLeaves", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1, }
                             
                         }
                    },

                    GatheringSiteType = new GatheringSiteType()
                    {
                        arc = new Arc()
                        {
                            Radius = 3f,
                            MinAngle = -180,
                            MaxAngle = 180
                        },
                        SeatSize = 5,
                        MaxVisitors = 2

                    },

                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_palewingweed_1_grown_summer", Bendyness = bendyness } }
                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {     
                            // GROWN
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_2_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_3_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_4_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_5_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_6_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.HasBranches)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_7_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.HasBranches)
                            },

                            // GROWN, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_2_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_3_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_4_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_5_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_6_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_7_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },


                            // GROWN, NO LEAVES
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_1_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_2_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_3_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_4_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_5_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_6_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_7_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7)
                            },

                            // GROWN, NO LEAVES, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_1_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_2_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_3_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_4_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_5_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_6_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_7_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Night)
                            },



                            // YOUNG
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_3_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_4_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_5_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Young)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_6_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Young)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_7_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Young)
                            },

                            // YOUNG, NIGHT
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_2_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_3_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_4_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_5_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                             new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_6_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Young, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweed_7_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Young, (int)StateModifier.Night)
                            },

                            // DEAD
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Dead)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Dead)
                            },

                           // DEAD, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour2, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour3, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },

                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour4, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour5, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour6, (int)StateModifier.Dead, (int)StateModifier.Night)
                            },
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palewingweeddead_2_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour7, (int)StateModifier.Dead, (int)StateModifier.Night)
                            }

                        }
                    },
                    PointLayoutType = new PointLayoutType() { IsBlocking = false }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region wingweed dead

                bendyness = 0.5f;
                tree = new EntityType("tree:wingweeddead")
                {
                    Name = "Wingweed - dead",
                    SummaryDescription = "Herbaceous plant with large, soft leaves. Common on firegrass",
                    Description = "\n BIOLOGY OVERVIEW\n Wingweed often grows in temperate, drier climes, which makes its presence amongst firegrass almost a constant; however the plant has an excellent plasticity and can be found in almost any habitat. Due to their proximity to the ground, the species generally have larger leaves with which to absorb sunlight.\n \n SURVIVAL GUIDE NOTES\n The leaves can serve as a temporary insulation as they are quite proficient at retaining heat.",
                    ThumbnailSmall = "HUD_thumbnail_wingweed",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 1f,
                        MatureAge = 1f,
                        MaxAge = 10f,
                        SizeImpact = 0.6f,
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f,
                  /*      Crops = new string[]{                           
                                
                                GameData.Instance.AllResourceTypes["crop:sticks"]
                        },

                        DefaultCrops = new[]
                         {                     
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                         }*/
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_wingweeddead_1_grown", Bendyness = bendyness } }
                        }
                    },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);

                #endregion
                #region desert wingweed dead

                bendyness = 0.5f;
                tree = new EntityType("tree:desertwingweeddead")
                {
                    Name = "Desert wingweed - dead",
                    SummaryDescription = "Herbaceous plant with large, soft leaves. This variant is common on dry soil",//
                    Description = "\n BIOLOGY OVERVIEW\n Wingweed often grows in temperate, drier climes, which makes its presence amongst firegrass almost a constant; however the plant has an excellent plasticity and the desert variant can be found on very dry soil. Due to their proximity to the ground, the species generally have larger leaves with which to absorb sunlight.\n \n SURVIVAL GUIDE NOTES\n The leaves can serve as a temporary insulation as they are quite proficient at retaining heat.",//TODO
                    ThumbnailSmall = "HUD_thumbnail_wingweed",
                    TreeType = new TreeType()
                    {
                        BulkPerSize = 1f,
                        MatureAge = 1f,
                        MaxAge = 10f,
                        SizeImpact = 0.6f,
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f,
                        /*      Crops = new string[]{                           
                                
                                      GameData.Instance.AllResourceTypes["crop:sticks"]
                              },

                              DefaultCrops = new[]
                               {                     
                                   new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                               }*/
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_palewingweeddead_1_grown", Bendyness = bendyness } }
                        }
                    },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);

                #endregion
                #region water cane
                bendyness = 0.3f;
                tree = new EntityType("tree:riveraxle")
                {
                    Name = "Water cane",
                    SummaryDescription = "Bamboo-like plant often found in riverbeds",
                    Description = "\n BIOLOGY OVERVIEW\n The water cane is a non-flowering perennial evergreen which grows in wet, nutrient-rich soil.\n \n SURVIVAL GUIDE NOTES\n  Its versatility in crafting is highly prized by us, as the hollow stems have exorbitant strength. Not only are the water cane's stems beneficial, but its seeds can be harvested for human consumption and its small, stiff leaves can be used as fletching.",
                //    Description = "",
                    ThumbnailSmall = "HUD_thumbnail_riveraxle",  
                    TreeType = new TreeType()
                    {                                  
                        BulkPerSize = 1f,
                        MatureAge = 5f,
                        MaxAge = 10f,
                        SizeImpact = 0.6f,
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{                                 
                                "crop:waterCaneLeaves",
                                "crop:sticks",
                                "crop:waterCaneStem"
                        },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:waterCaneLeaves", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:waterCaneStem", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                         }
                    },
                    RenderableType = new RenderableType() {  
                        DefaultClientState = 
                
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_riveraxle_1_grown_summer", Bendyness = bendyness } }
                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {   
               //GROWN, BRANCHES                      
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_riveraxle_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches)
                            },

       //GROWN, BRANCHES, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_riveraxle_1_grown_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.HasBranches, (int)StateModifier.Night)
                            },


        // GROWN, CUT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_riveraxle_1_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1)
                            },
        // GROWN, CUT, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_riveraxle_1_grown_cut", Bendyness = 0.1f }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Night)
                            },

            //YOUNG
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_riveraxle_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young)
                            },

            //YOUNG, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_riveraxle_1_young_summer", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Young, (int)StateModifier.Night)
                            },

            // DEAD
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_riveraxledead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead)
                            },

            // DEAD, NIGHT
                            new ClientStateInfo()
                            {
                                RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_riveraxledead_1_grown", Bendyness = bendyness }},                                 
                                Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Flavour1, (int)StateModifier.Dead, (int)StateModifier.Night)
                            }
                        }
                },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion
                #region water cane dead
                bendyness = 0.3f;
                tree = new EntityType("tree:riveraxledead")
                {
                    Name = "Water cane - dead",
                    SummaryDescription = "Bamboo-like plant often found in riverbeds",
                    Description = "\n BIOLOGY OVERVIEW\n The water cane is a non-flowering perennial evergreen which grows in wet, nutrient-rich soil.\n \n SURVIVAL GUIDE NOTES\n  Its versatility in crafting is highly prized by us, as the hollow stems have exorbitant strength. Not only are the water cane's stems beneficial, but its seeds can be harvested for human consumption and its small, stiff leaves can be used as fletching.",
                    ThumbnailSmall = "HUD_thumbnail_riveraxle",
                    TreeType = new TreeType()
                    {                       
                        BulkPerSize = 1f,
                        MatureAge = 5f,
                        MaxAge = 10f,
                        SizeImpact = 0.6f,
                        FibrousPercentageOfTotalMass = 0f,
                        LumberPercentageOfFiberMass = 0f,
                        Crops = new string[]{                                
                              "crop:sticks",
                              "crop:waterCaneStem",  
                              "crop:waterCaneSeeds"
                        },

                        DefaultCrops = new[]
                         {                            
                             new DefaultCrops(){ KeyName = "crop:sticks", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:waterCaneStem", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 },
                             new DefaultCrops(){ KeyName = "crop:waterCaneSeeds", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                         }
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_riveraxledead_1_grown", Bendyness = bendyness } }
                        }
                    },
                    PointLayoutType = new PointLayoutType() { }
                };
                listOfEntityTypes.Add(tree);
                #endregion

            #region Desert Bramble
                tree = new EntityType("tree:desertbrambletiny")
                {
                    Name = "Desert bramble - small",
                    SummaryDescription = "Tangled bushes that form a dense barrier.", //mostly copied from iron bramble
                    Description = "\n BIOLOGY OVERVIEW\n Desert bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.",
                    ThumbnailSmall = "HUD_thumbnail_bramble",
                    TreeType = new TreeType()
                    {
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                    /*    Crops = new string[]{   "crop:thorns"     },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:thorns", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                  
                         }*/
                    },

                    GatheringSiteType = new GatheringSiteType()
                    {
                        arc = new Arc()
                        {
                            Radius = 8f,
                            MinAngle = -180,
                            MaxAngle = 180
                        },
                        SeatSize = 1,
                        MaxVisitors = 2

                    },

                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_palebrambletiny_1_grown", Bendyness = bendyness } },

                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                     
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palebrambletiny_1_young", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Young)
                        }}
                    },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 14)
                                {
                                    Offset = new Vector2(-1,-4)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);

                /*tree = new EntityType("tree:bramble")
                {
                    Name = "Bramble",
                    TreeType = new TreeType()
                    {
                        SpriteName = "bramble",
                        Bendyness = 0.2f,
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f
                    },
                    RenderableType = new RenderableType()
                    {
                        RenderAsBillboardType = new RenderAsBillboardType[]
                {new RenderAsBillboardType()
                {
                   /* SpriteName = "daysheen"*/
                /*}
                }
                    },
                    DefaultSimState = new SimStateInfo()
                {                	
                    GeometryLayoutType = new GeometryLayoutType() //
                    {
                        Shapes = new CollideShape2D[] 
                        {
                            new CollideShape2D(Vector2.Zero, 17)
                            {
                                Offset = new Vector2(-1,-11)
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);*/

                bendyness = 0.2f;
                tree = new EntityType("tree:desertbramblesmall")
                {
                    Name = "Desert bramble - larger",
                    SummaryDescription = "Tangled bushes that form a dense barrier.", //mostly copied from iron bramble
                    Description = "\n BIOLOGY OVERVIEW\n Desert bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.",
                    TreeType = new TreeType()
                    {
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                   /*     Crops = new string[]{        "crop:thorns"           },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:thorns", MinItemsForFullGrownPlant = 0, MaxItemsForFullGrownPlant = 1 }
                  
                         }*/
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_palebramblesmall_1_grown", Bendyness = bendyness } },

                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                     
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palebramblesmall_1_young", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Young)
                        }}
                    },
                    DefaultSimState = new SimStateInfo()
                    {                	
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 19)
                                {
                                    Offset = new Vector2(0,-8)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);

                bendyness = 0.2f;
                tree = new EntityType("tree:desertbramblemedium")
                {
                    Name = "Desert bramble - big",
                    SummaryDescription = "Tangled bushes that form a dense barrier.", //mostly copied from iron bramble
                    Description = "\n BIOLOGY OVERVIEW\n Desert bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.",
                    TreeType = new TreeType()
                    {
                        BulkPerSize = 10f,
                        MatureAge = 6f,
                        MaxAge = 40f,
                        SizeImpact = 1f,
                        FibrousPercentageOfTotalMass = 0.2f,
                        LumberPercentageOfFiberMass = 0f,
                   /*     Crops = new string[]{  "crop:thorns"      },

                        DefaultCrops = new[]
                         {
                             new DefaultCrops(){ KeyName = "crop:thorns", MinItemsForFullGrownPlant = 1, MaxItemsForFullGrownPlant = 1 }
                  
                         }*/
                    },
                    RenderableType = new RenderableType()
                    {
                        DefaultClientState = new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[] { new RenderAsBillboardType() { AssetName = "tree_palebramblemedium_1_grown", Bendyness = bendyness } },

                        },
                        ClientStateConditions = new ClientStateInfo[]
                        {                     
                        new ClientStateInfo()
                        {
                            RenderAsBillboardType = new RenderAsBillboardType[]{new RenderAsBillboardType(){ AssetName = "tree_palebramblemedium_1_young", Bendyness = bendyness }},                                 
                            Conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Young)
                        }}
                    },
                    DefaultSimState = new SimStateInfo()
                    {
                        GeometryLayoutType = new GeometryLayoutType() //
                        {
                            Shapes = new CollideShape2D[] 
                            {
                                new CollideShape2D(Vector2.Zero, 23)
                                {
                                    Offset = new Vector2(0,-16)
                                }
                            }
                        }
                    }
                };
                listOfEntityTypes.Add(tree);
            #endregion

        }
    }
}

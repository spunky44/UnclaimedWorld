using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Templates;

namespace UWGame.SimSide.AllGameData
{
    public class TraitTemplateLoader
    {
        public static List<TraitTemplate> Init()
        {
            List<TraitTemplate> list = new List<TraitTemplate>();

           
            //see edges by searching for MediumSkillDistribution. it overlaps the lowskill and highskill, currently.//           
            #region "electronicsSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "electronicsSpecialist",
                ExpertSkills = new string[] { "electronics", "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] {  },
                MediumSkills = new string[] { "bushcraft", "shooting", "construction", "mechanics" },
                LowSkills = new string[] { "menial", "medicine", "butchering", "farming", "armedMelee", "unarmedFighting", "cooking", "archery", "sneaking", "hunting", "fishing", "foraging", "weaving", "carpentry" },
                ZeroSkills = new string[] { "biology", "psychology", "smithing", "chemistry" } // smithing zero skill if we want the player to need immigration. (Electronics required for radio hut)

            });
            #endregion
            #region "mechanicsSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "mechanicsSpecialist",
                ExpertSkills = new string[] {  "mechanics", "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] { },
                MediumSkills = new string[] { "bushcraft", "shooting", "construction", "electronics", "carpentry" },
                LowSkills = new string[] { "smithing", "menial", "medicine", "butchering", "farming", "armedMelee", "unarmedFighting", "cooking", "archery", "sneaking", "hunting", "fishing", "foraging", "weaving" },
                ZeroSkills = new string[] { "biology", "psychology",  "chemistry" } // smithing zero skill if we want the player to need immigration. (Electronics required for radio hut)

            });
            #endregion
            #region "smithingSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "smithingSpecialist",
                ExpertSkills = new string[] { "smithing", "grasping", "fruitPicking","weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] { "bushcraft"  },
                MediumSkills = new string[] {  "menial", "armedMelee", "unarmedFighting", "construction", "carpentry" },
                LowSkills = new string[] { "butchering",  "farming", "shooting", "cooking", "archery", "sneaking", "hunting", "fishing", "foraging", "chemistry", "mechanics", "weaving" },
                ZeroSkills = new string[] { "medicine", "biology", "psychology", "electronics", } 

            });
            #endregion
            #region "farmingSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "farmingSpecialist",
                ExpertSkills = new string[] { "farming", "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] {  "menial" },
                MediumSkills = new string[] { "bushcraft", "construction", "unarmedFighting", "butchering", "foraging", "carpentry" },
                LowSkills = new string[] { "smithing",  "shooting", "cooking", "archery", "armedMelee", "sneaking", "hunting", "fishing",  "biology", "weaving" },
                ZeroSkills = new string[] { "medicine",  "psychology", "electronics", "chemistry", "mechanics" } 

            });
            #endregion
            #region "constructionSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "constructionSpecialist",
                ExpertSkills = new string[] { "construction",  "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] { "bushcraft", },
                MediumSkills = new string[] {  "menial", "armedMelee", "carpentry" },
                LowSkills = new string[] { "farming",  "shooting", "cooking", "archery", "sneaking", "unarmedFighting", "hunting", "fishing", "foraging", "butchering", "weaving" },
                ZeroSkills = new string[] { "medicine", "psychology", "smithing", "biology", "electronics", "chemistry", "mechanics" }

            });
            #endregion
            #region "huntingSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "huntingSpecialist",
                ExpertSkills = new string[] { "hunting", "archery", "shooting", "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] { "bushcraft", "armedMelee",  "sneaking", "foraging", "butchering",  },
                MediumSkills = new string[] {  "menial",  "unarmedFighting",  "fishing",  },
                LowSkills = new string[] { "farming", "cooking", "biology", "construction", "carpentry", "weaving" },
                ZeroSkills = new string[] {  "psychology", "smithing", "medicine", "electronics", "chemistry", "mechanics" }

            });
            #endregion
            #region "menialSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "menialSpecialist",
                ExpertSkills = new string[] { "menial",  "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] {  },
                MediumSkills = new string[] { "unarmedFighting", "armedMelee",  "sneaking",  "butchering", "shooting", },
                LowSkills = new string[] { "farming", "cooking", "hunting", "construction", "fishing", "bushcraft", "foraging", "archery", "carpentry", "weaving"},
                ZeroSkills = new string[] {  "psychology", "smithing", "biology", "medicine", "electronics", "chemistry", "mechanics" }

            });
            #endregion
            #region "cookingSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "cookingSpecialist",
                ExpertSkills = new string[] { "cooking", "butchering", "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] { },
                MediumSkills = new string[] { "unarmedFighting", "armedMelee", "foraging", "shooting", "menial", "psychology", },
                LowSkills = new string[] { "farming", "hunting", "construction", "archery", "sneaking", "fishing", "bushcraft", "medicine", "weaving", "carpentry" },
                ZeroSkills = new string[] {  "smithing", "biology", "electronics", "chemistry", "mechanics" }

            });
            #endregion
            #region "bushcraftSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "bushcraftSpecialist",
                ExpertSkills = new string[] { "bushcraft",   "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] { "foraging", "fishing", "hunting", "archery", },
                MediumSkills = new string[] { "unarmedFighting", "armedMelee",  "shooting", "menial", "construction",   "sneaking",  "cooking", "butchering", "carpentry" },
                LowSkills = new string[] { "farming", "medicine", "weaving" },
                ZeroSkills = new string[] { "psychology", "smithing", "biology", "electronics", "chemistry", "mechanics" }

            });
            #endregion
            #region "securitySpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "securitySpecialist",
                ExpertSkills = new string[] { "shooting", "armedMelee",  "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] { "archery", "unarmedFighting", "sneaking", },
                MediumSkills = new string[] { "menial", "medicine", "psychology", "foraging", "fishing", "hunting", "bushcraft", },
                LowSkills = new string[] { "farming", "construction", "cooking", "butchering", "weaving", "carpentry" },
                ZeroSkills = new string[] {  "smithing", "biology", "electronics", "chemistry", "mechanics" }

            });
            #endregion
            #region "medicineSpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "medicineSpecialist",
                ExpertSkills = new string[] { "medicine", "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] { "biology", "psychology", },
                MediumSkills = new string[] { "bushcraft", "shooting", "butchering", },
                LowSkills = new string[] { "menial", "construction", "farming", "armedMelee", "unarmedFighting", "cooking", "archery", "sneaking", "hunting", "fishing", "foraging", "chemistry", "weaving", "carpentry"},
                ZeroSkills = new string[] { "smithing",  "electronics",  "mechanics" } 

            });
            #endregion

            #region "chemistrySpecialist"
            list.Add(new TraitTemplate()
            {
                KeyName = "chemistrySpecialist",
                ExpertSkills = new string[] { "chemistry", "grasping", "fruitPicking", "weeding", }, // humans should always be experts at basic skills shared with robots
                HighSkills = new string[] { "biology", },
                MediumSkills = new string[] { "bushcraft", "shooting", "butchering", "medicine", "cooking", "farming", },
                LowSkills = new string[] { "psychology", "menial", "construction", "armedMelee", "unarmedFighting",  "archery", "sneaking", "hunting", "fishing", "foraging", "weaving", "carpentry" },
                ZeroSkills = new string[] { "smithing", "electronics", "mechanics",  }

            });
            #endregion

            return list;

        }

    }
}

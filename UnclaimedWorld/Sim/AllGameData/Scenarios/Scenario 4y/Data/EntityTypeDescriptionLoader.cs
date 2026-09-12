using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4y.Data
{
    public class EntityTypeDescriptionLoader
    {
        public static List<EntityTypeDescription> Init()
        {
            List<EntityTypeDescription> list = new List<EntityTypeDescription>();

            list.Add(new EntityTypeDescription()
            {
                KeyName = "personDescription",
                EntityType = "entity:human",
                EntityName = "Colony member",
                SummaryDescription = "Member of the community founded by Castor Hernes",
                Description = ""
            });


            list.Add(new EntityTypeDescription()
            {
                KeyName = "inactivatedFoodCoolerUnitDescription",
                EntityType = "item:inactivatedFoodCoolerUnit",
                EntityName = "Refrigerator unit (inactivated)",
                SummaryDescription = "Used for cooling a food container",
                Description = "Can be placed in a food container to keep temperature at 5 degrees Celsius. Would have a battery life of 2-5 months depending on environment." 
            });


            list.Add(new EntityTypeDescription()
            {
                KeyName = "activatedFoodCoolerUnitDescription",
                EntityType = "item:activatedFoodCoolerUnit",
                EntityName = "Refrigerator unit (activated)",
                SummaryDescription = "Used for cooling a food container",
                Description = "The device is activated and will keep the surrounding temperature at 5 degrees Celsius. Will run out of battery in 2-5 months depending on environment." //MP this copied from air condition unit
            });

            list.Add(new EntityTypeDescription()
            {
                KeyName = "cooledFoodCacheDescription",
                EntityType = "structure:cooledFoodCache",
                EntityName = "Cooled food cache", //same
                SummaryDescription = "Cooled with a refrigerator unit",
                Description = "It is possible to refrigerate food by storing it in a hole together with a refrigerator unit. The hole must be lined with large stones and covered with spoak leaves and rocks to keep animals out."
            });


            list.Add(new EntityTypeDescription()
            {
                KeyName = "scrapMetalDescription",
                EntityType = "item:scrapMetal",
                EntityName = "Scrap metal", //mp same
                SummaryDescription = "Pieces of various types of metal",
                Description = "Has a varying quality. Comes from different sources such as vehicles, machines and buildings." 
            });

            list.Add(new EntityTypeDescription()
            {
                KeyName = "panelScrapsDescription",
                EntityType = "item:panelScraps",
                EntityName = "Panel scraps", //mp same
                SummaryDescription = "Thermoplastics/composite panels",//mp same
                Description = "Pieces of interior panels made of composite materials and thermoplastics. They can probably find some use for building improvised shelter."
            });

            list.Add(new EntityTypeDescription()
            {
                KeyName = "lean-toScrapsDescription",
                EntityType = "structure:lean-toScraps",
                EntityName = "Lean-to (Scraps)", //mp same
                SummaryDescription = "An improvised 2-person shelter made from panel scraps",
                Description = "Pieces of interior panels made of composite materials and thermoplastics. They can probably find some use for building improvised shelter."//mp same
            });

            list.Add(new EntityTypeDescription()
            {
                KeyName = "A-frameScrapsDescription",
                EntityType = "structure:A-frameScraps",
                EntityName = "A-frame (Scraps)", //mp same
                SummaryDescription = "A crude 1-person shelter made from panel scraps",
                Description = "Has room for one."//mp same
            });


 /*           list.Add(new EntityTypeDescription()
            {
                KeyName = "textileDescription",
                EntityType = "item:textile",
                EntityName = "Textile", //mp same
                SummaryDescription = "Piece of permeable synthetic fabric",//mp same
                Description = "Can find use where a fine mesh fabric is needed which allows air and liquid to pass through." //
            });
*/
           /* list.Add(new EntityTypeDescription()
            {
                KeyName = "strongBugNetDescription",
                EntityType = "item:strongBugNet",//mp same
                EntityName = "Strong bug net",//mp same
                SummaryDescription = "Used for catching small flyers and fish minnows", 
                Description = "A simple net at the end of a long handle." 
            });*/


            list.Add(new EntityTypeDescription()
            {
                KeyName = "superconductingWireDescription",
                EntityType = "item:superconductingWire",
                EntityName = "Superconducting wire", //mp same
                SummaryDescription = "Pieces of wire used in advanced electrical systems",
                Description = "Although designed for electric power transmission, this wire has enough ductility and tensile strength that it may find use in simple construction tasks." ////mp same
            });

            list.Add(new EntityTypeDescription()
            {
                KeyName = "improvisedCookingPotDescription",
                EntityType = "item:improvisedCookingPot",
                EntityName = "Improvised cooking pot",//mp same
                SummaryDescription = "Metal pot made from scrap metal",
                Description = "This cooking pot is better than nothing." ////mp same
            });




            return list;

        }

      


    }
}

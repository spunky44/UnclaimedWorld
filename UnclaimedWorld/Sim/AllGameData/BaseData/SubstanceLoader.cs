using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Substances;

namespace UWGame.SimSide.AllGameData
{
    public class SubstanceLoader
    {
        public static List<SubstanceType> Init()
        {
            List<SubstanceType> list = new List<SubstanceType>();


            list.Add(new SubstanceType()
            {
                KeyName = "gold",
                Name = "Gold"
            });

            list.Add(new SubstanceType()
            {
                KeyName = "iron",
                Name = "Iron"
            });

            list.Add(new SubstanceType()
            {
                KeyName = "dirt",
                Name = "Dirt"
            });

            list.Add(new SubstanceType()
            {
                KeyName = "meat",
                Name = "Meat"                   
                    
            });

            list.Add(new SubstanceType()
            {
                KeyName = "bones",
                Name = "Bones"

            });

            list.Add(new SubstanceType()
            {
                KeyName = "hide",
                Name = "Hide"

            });


            list.Add(new SubstanceType()
            {
                KeyName = "guts",
                Name = "Guts"

            });

            list.Add(new SubstanceType()
            {
                KeyName = "plating",
                Name = "Plating"

            });

            list.Add(new SubstanceType()
            {
                KeyName = "shell",
                Name = "Shell"

            });

           /* list.Add(new SubstanceType()
            {
                KeyName = "shell",
                Name = "Shell"

            });*/

            return list;
        }


    }
}

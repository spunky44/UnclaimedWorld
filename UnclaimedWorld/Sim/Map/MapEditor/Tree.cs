using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Maps.MapEditor
{
    public class Tree
    {
        public AgeGroup? AgeGroup = null; // AgeGroup.Grown;


        public InSeason InSeason = InSeason.Summer;

        /// <summary>
        /// in years
        /// </summary>
        public float? AgeInYears;

        // distort width/height by factor:
        public float ShapeFactor;

        /// <summary>
        /// append number to sprite name to get the correct sprite...
        /// </summary>
        public int Flavour;
        
        /// <summary>
        /// 0 - unlimited. 1 = size at maturity on average.
        /// </summary>
       // public float? Size;


        public bool ShouldSerializeAgeInYears()
        {
            return AgeInYears != null;
        }

        public bool ShouldSerializeAgeGroup()
        {
            return AgeGroup != null;
        }

        public void SetTreeComponentPreInit(Trees.Tree treeComponent)
        {
            // NEW: if the designer only specified age group young/mature, fill in with random age and size according to entity type data.
           
            if (AgeGroup.HasValue)
            {
                treeComponent.SetAgePreInit(AgeGroup.Value);
            }
            else if (AgeInYears.HasValue)
            {
                treeComponent.SetAgePreInit(AgeInYears.Value);
            }
            else
            {
                treeComponent.SetAgePreInit(); // fully random
            }

            // size and bulk should be set from Age... in Initialize()
           
            treeComponent.Flavour = Flavour;
            treeComponent.InSeason = InSeason;
          
            treeComponent.ShapeFactor = ShapeFactor;

            //treeComponent.Size = Size;

        }

    }
}

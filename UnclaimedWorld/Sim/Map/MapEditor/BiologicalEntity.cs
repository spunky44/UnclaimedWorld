using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Maps.MapEditor
{
    /// <summary>
    /// contained in EntityData
    /// </summary>
    public class BiologicalEntity
    {
        /// <summary>
        /// there is also age settings in CultureTemplate, but that is not really needed for critters, just means more work
        /// </summary>
        public NormalDistribution AgeInYears;
       // public float? AgeInYears;

        /// <summary>
        /// if no age property is defined, the age will be totally random
        /// </summary>
        public AIAgeGroup? AgeGroup;


        public string CasteKey;

        public string RaceKey;

        /// <summary>
        /// will override race, age and caste textures, if specified
        /// </summary>
        public string ModelTextureName;



        /// <summary>
        /// move to intelligence..
        /// </summary>
        public SerializableDictionary<string, float> Skills;

        public NormalDistribution StomachContent;


        /// <summary>
        /// edges/buckets are allowed.
        /// </summary>
        public StringChance[] TraitTemplates;

        /// <summary>
        /// edges/buckets are allowed.
        /// </summary>     
        public StringChance[] CultureTemplates;

        /// <summary>
        /// i want to make it possible to combine entityData with other sources, so there are optional params here...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="age"></param>
        public void FillEntity(Entity entity, float? age = null, string casteKey = null)
        {

            if (TraitTemplates != null)
            {
                int index;
                StringChance trait = Common.GetStairStepIndex(TraitTemplates, out index, The.Sim.GameplayRandomGenerator);

                TraitTemplate template = GameData.Instance.AllTraitTemplates[trait.String];

                template.FillEntity(entity);

            }
            else if (Skills != null)
            {
                foreach (var skill in Skills)
                {
                    entity.Intelligence.SetSkill(skill.Key, skill.Value);
                }
            }


            Entities.Biological.BiologicalEntity bioComponent;
            entity.Find(out bioComponent);

            if (CultureTemplates != null)
            {
                int index;
                StringChance trait = Common.GetStairStepIndex(CultureTemplates, out index, The.Sim.GameplayRandomGenerator);

                CultureTemplate template = GameData.Instance.AllCultureTemplates[trait.String];

                template.FillEntity(entity);

            }
            else
            {
                SetCaste(CasteKey ?? casteKey, bioComponent);


                if (!string.IsNullOrEmpty(RaceKey))
                {
                    if (RaceKey == "ManOchreClothesBlackHairTexture")
                    {
                                            
                    }

                    bioComponent.SetRaceOnNewEntity(RaceKey);
                }
                // race can be null.

                float? ageInYears = null;

                if (this.AgeInYears != null)
                {
                    ageInYears = (float)AgeInYears.GetRandomValue(The.Sim.GameplayRandomGenerator);
                    bioComponent.SetAgePreInit(ageInYears);
                }
                else if (this.AgeGroup != null)
                {
                    bioComponent.SetAgePreInit(null, AgeGroup.Value);
                }
                else if (age.HasValue)
                {
                    bioComponent.SetAgePreInit(age);
                }
                else
                {
                    bioComponent.SetAgePreInit(null);
                }
                
            }

            if (!string.IsNullOrEmpty(ModelTextureName))
            {
                bioComponent.ModelTextureName = ModelTextureName;
            }
        }

      /*  public static void SetAge(float? ageInYears, Entities.Biological.BiologicalEntity bioComponent)
        {
            // must be set before Initialize:           
            if (ageInYears.HasValue) 
            {
                bioComponent.SetAgePreInit(ageInYears);
            }
            else
            {
                bioComponent.SetAgePreInit(null); // set random age
            }
        }*/

        public static void SetCaste(string casteKey, Entities.Biological.BiologicalEntity bioComponent)
        {
            if (!string.IsNullOrEmpty(casteKey))
            {
                bioComponent.SetCasteOnNewEntity(casteKey);

                if (bioComponent.CasteType == null)
                    throw new Exception("Unknown caste in saved map entity: " + casteKey + " (Set Caste key to null to pick a random caste)");
            }
            else
            {
                // caste must be set..
                bioComponent.SetRandomCaste();
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.Overland.Templates
{
   /// <summary>
   /// Coupling template and children makes it possible to get more coherence.
   /// But requires more data entries - allow inline children too?
   /// 
   /// Decoupling them could be more convenient for the designer too.
   /// 
   /// Perhaps allow both?
   /// 
   /// maybe merge with SiteData
   /// should describe a type of geographical site, mountain, farming, trade, fishing...
   /// 
   /// should site and allegiance be coupled or be independent? Coupled for now...
   /// 
   /// Suggestion: make a Scale factor that affects all nested allegiances and expeditions. Scale affects number of immigrants and trade goods. This way, less data needs to be defined.
   /// </summary>
    public class SiteTemplate: IGameData
    {
        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public bool DeleteRecord
        {
            get;
            set;
        }

        /// <summary>
        /// edges/buckets are allowed.
        /// 
        /// AllegianceData keys
        /// 
        /// allegianceData contain expeditions
        /// </summary>
        public StringChanceSet[] Allegiances;

        

        // Random site names...
        public string[] Names;


        public string Description;


        /// <summary>
        /// make a template for each size... or randomize?
        /// 
        /// Site names do not need to express size... but descriptions do...
        /// </summary>
        public float? SizeFactor;

          
        public void FillSite(Site site, string allegianceKeyName = null, string expeditionKeyName = null)
        {           
            if (Names != null)
            {
                // remove already used names:
                List<string> namesToUse = null;
                foreach (var item in Names)
                {
                    if (!The.Sim.World.AllSites.Any(s => s.Value.Name == item))
                    {
                        Common.AddToList(ref namesToUse, item);
                    }
                }

                if (namesToUse != null)
                {
                    site.Name = Common.GetRandomListMember(namesToUse, The.Sim.GameplayRandomGenerator);
                }
                else
                {
                    site.Name = KeyName; // ??
                }              
            }

            if (Description != null)
            {
                site.Description = Description;
            }

            if (Allegiances != null)
            {
                foreach (var item in Allegiances)
                {
                    int index;
                    StringChance trait = Common.GetStairStepIndex(item.Chances, out index, The.Sim.GameplayRandomGenerator);

                    AllegianceData allegianceData = GameData.Instance.AllAllegianceData[trait.String];

                    Allegiance allegiance = Allegiance.CreateFromAllegianceData(allegianceData, site, SizeFactor, allegianceKeyName, expeditionKeyName);
                    
                }

               /*
                int index;
                StringChance trait = Common.GetStairStepIndex(AllegianceTemplates, out index, The.Sim.GameplayRandomGenerator);

                AllegianceData allegianceData = GameData.Instance.AllAllegianceData[trait.String];

                Allegiance allegiance = Allegiance.CreateFromAllegianceData(allegianceData);
                */
               

              /*  Allegiance allegiance = Allegiance.CreateFromAllegianceData();

                int index;
                StringChance trait = Common.GetStairStepIndex(AllegianceTemplates, out index, The.Sim.GameplayRandomGenerator);

                AllegianceTemplate template = GameData.Instance.AllAllegianceTemplates[trait.String];

                template.FillAllegiance(allegiance);
                */
            }


            /*SetSkills(ExpertSkills, entity, GameData.Instance.Constants.ExpertSkillDistribution);
            SetSkills(HighSkills, entity, GameData.Instance.Constants.HighSkillDistribution);
            SetSkills(MediumSkills, entity, GameData.Instance.Constants.MediumSkillDistribution);
            SetSkills(LowSkills, entity, GameData.Instance.Constants.LowSkillDistribution);
            SetSkills(ZeroSkills, entity, GameData.Instance.Constants.ZeroSkillDistribution);*/
        }
      
      /*  public List<Tuple<string, float>> GenerateSkills()
        {
           

        }*/


        private bool HasSkill(string[] group, SkillType skill)
        {
            if (group != null)
            {
                return group.Any(s => s == skill.KeyName);
            }

            return false;
        }



        public void PreInitValidate(ref List<string> errors)
        {
          
        }

        public void Initialize()
        {
           
        }

        public void PostInitValidate(ref List<string> errors)
        {
          
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {
           
        }

        public void PostDataCompleteInitialize()
        {
            
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
           

        }
    }
}

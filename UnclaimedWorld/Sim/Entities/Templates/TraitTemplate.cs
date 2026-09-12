using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Templates
{
    /// <summary>
    /// referenced within EntityData
    /// 
    /// NOT: sex, face, race...
    /// 
    /// perhaps age and clothes
    /// </summary>
    public class TraitTemplate: IGameData
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


        public string[] ExpertSkills;

        public string[] HighSkills;

        public string[] MediumSkills;

        public string[] LowSkills;
        
        public string[] ZeroSkills;


        
  

        private void SetSkills(string[] group, Entity entity, NormalDistribution distribution)
        {
            foreach (var skill in group)
            {
                entity.Intelligence.SetSkill(skill, (float)distribution.GetRandomValue(The.Sim.GameplayRandomGenerator, true));
            }

        }
          
        public void FillEntity(Entity entity)
        {
            SetSkills(ExpertSkills, entity, GameData.Instance.Constants.ExpertSkillDistribution);
            SetSkills(HighSkills, entity, GameData.Instance.Constants.HighSkillDistribution);
            SetSkills(MediumSkills, entity, GameData.Instance.Constants.MediumSkillDistribution);
            SetSkills(LowSkills, entity, GameData.Instance.Constants.LowSkillDistribution);
            SetSkills(ZeroSkills, entity, GameData.Instance.Constants.ZeroSkillDistribution);
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
            foreach (var item in GameData.Instance.AllSkillTypes)
            {
                if (HasSkill(ExpertSkills, item.Value))
                    continue;
                if (HasSkill(HighSkills, item.Value))
                    continue;
                if (HasSkill(MediumSkills, item.Value))
                    continue;
                if (HasSkill(LowSkills, item.Value))
                    continue;
                if (HasSkill(ZeroSkills, item.Value))
                    continue;

                EntityType.CreateValidationError(ref listOfErrors, "Skill not represented: " + item.Key);
            }

        }
    }
}

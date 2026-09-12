using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.Entities.Templates
{
    /// <summary>
    /// sex, face, race...
    /// 
    /// </summary>
    public class CultureTemplate: IGameData
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

       // public Reproduction Reproduction;
        public string CasteKey;
        public string RaceKey; // RaceType RaceType;

        public NormalDistribution AgeInYears;
        
       // public string[] Portraits; // derived from the other properties

        public int NoOfPortraitFlavours = 1;

        public string[] UncommonFirstNames;
        public string[] CommonFirstNames;

        public string[] UncommonLastNames;
        public string[] CommonLastNames;

        [XmlIgnore]
        List<StringChance> firstNames;
        List<StringChance> lastNames;


        public void FillEntity(Entity entity)
        {
            string firstName = null, lastName = null;
            int index;

          /*  Person personComponent = entity.PersonEntity;
            if (personComponent != null)
            {

                firstName = Common.GetStairStepIndex(firstNames, out index, The.Sim.GameplayRandomGenerator).String;
                if (lastNames != null)
                {
                    lastName = Common.GetStairStepIndex(lastNames, out index, The.Sim.GameplayRandomGenerator).String;
                }

                personComponent.SetName(firstName, lastName);
            }*/

            Intelligence intelligenceComponent = entity.Intelligence;
            if (intelligenceComponent != null)
            {
                firstName = Common.GetStairStepIndex(firstNames, out index, The.Sim.GameplayRandomGenerator).String;
                if (lastNames != null)
                {
                    lastName = Common.GetStairStepIndex(lastNames, out index, The.Sim.GameplayRandomGenerator).String;
                }

                intelligenceComponent.SetName(firstName, lastName);
            }

            Entities.Biological.BiologicalEntity bioComponent;
            entity.Find(out bioComponent);

            UWGame.SimSide.Maps.MapEditor.BiologicalEntity.SetCaste(CasteKey, bioComponent);
            
            if (!string.IsNullOrEmpty(RaceKey))
            {
                bioComponent.SetRaceOnNewEntity(RaceKey);
            }
            // race can be null.

            float? ageInYears = null;

            if (this.AgeInYears != null)
            {
                ageInYears = (float)AgeInYears.GetRandomValue(The.Sim.GameplayRandomGenerator);
            }

            bioComponent.SetAgePreInit(ageInYears);
            //UWGame.SimSide.Maps.MapEditor.BiologicalEntity.SetAge(ageInYears, bioComponent);

            if (entity.PersonEntity != null)
            {               
                entity.PersonEntity.PortraitFlavour = 1 + The.Client.ClientRandomGenerator.Next(NoOfPortraitFlavours, "FillEntity");
            }
        }



        public void PreInitValidate(ref List<string> errors)
        {

        }

        const float commonChance = 0.2f;
        const float uncommonChance = 0.1f;


        public void Initialize()
        {
            InitializeNames(ref firstNames, CommonFirstNames, UncommonFirstNames);

            if (CommonLastNames != null || UncommonLastNames != null)
            {
                InitializeNames(ref lastNames, CommonLastNames, UncommonLastNames);
            }
        }

        private static void InitializeNames(ref List<StringChance> list, string[] commonNames, string[] uncommonNames)
        {
            list = new List<StringChance>();
            float currentEdge = 0f;
            if (uncommonNames != null)
            {
                currentEdge += uncommonChance;
                foreach (var item in uncommonNames)
                {
                    list.Add(new StringChance() { String = item, Edge = currentEdge });
                    currentEdge += uncommonChance;
                }
            }

            if (commonNames != null)
            {
                currentEdge += commonChance;
                foreach (var item in commonNames)
                {
                    list.Add(new StringChance() { String = item, Edge = currentEdge });
                    currentEdge += commonChance;
                }
            }

            // normalize:
            float factorToDivideBy = currentEdge;

            foreach (var item in list)
            {
                item.Edge /= factorToDivideBy;
            }
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

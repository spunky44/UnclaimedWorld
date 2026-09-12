using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.Scenarios
{
    /// <summary>
    /// header file: scenario.xml
    /// data file: scenarioData.xml
    /// </summary>
    public class ScenarioData
    {
        /// <summary>
        /// must point to a spawn world action
        /// </summary>
        public string SpawnWorldAction;

        /// <summary>
        /// must point to a spawn site action
        /// </summary>
        public string SpawnSiteAction;


        /// <summary>
        /// actions to perform regardless of difficulty 
        /// </summary>
        public /*EventActionType[]*/ string[] Actions;
        
        /// <summary>
        /// global events to register regardless of difficulty 
        /// </summary>
        public string[] ConditionalEvents;


        /// <summary>
        /// the main difficulty settings - each can refer to options in the option set
        /// also defines the custom difficulties that can be used
        /// </summary>
        public Difficulty[] MainDifficultySettings;

        /// <summary>
        /// setting this to false will hide the CUSTOM radio button, even though there are options to choose from
        /// </summary>
        public bool CustomEnabled = true;

        /// <summary>
        /// the categories that the player sees when he selects "Custom"
        /// </summary>
        public OptionSet[] OptionSets;


        public bool EnableMissions = true;
        public bool EnableGraphs = true;
        public bool EnableContacts = true;
        public bool EnablePersonell = true;
        public bool EnableWorldMap = true;
        public bool EnablePolicy = true;
        public bool EnableLedger = true;

        public string LoadingBackgroundImage;

        /// <summary>
        /// the image has to be in the CRT Content folder (rebuild the GUI_CRT_Sprites.xml spritesheet)
        /// </summary>
        public string LoadingDialogImage;

        public string LoadingDialogHeading;
        public string LoadingDialogText;

        /// <summary>
        /// the image has to be in the Content\Scenarios\Default\GUI 
        /// </summary>
        public string WorldMapImage;

        [XmlIgnore]
        public Texture2D WorldMapTexture;

        public void Initialize()
        {
            if (OptionSets != null)
            {
               
                foreach (var item in OptionSets)
                {
                    item.Initialize();
                }

                var ordered = OptionSets.OrderBy(o => o.DisplayGroup);

                // place them back after ordering them:
                OptionSets = ordered.ToArray();
                                
            }
        }


        /// <summary>
        /// for in-game content only?
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            if (WorldMapImage != null)
            {
                WorldMapTexture = content.Load<Texture2D>(WorldMapImage);
            }
        }


        public void PostInitValidate(List<string> listOfErrors)
        {
            if (MainDifficultySettings == null)
            {
                EntityType.ValidateRequiredValue(ref listOfErrors, "MainDifficultySettings", false);
            }
            else
            {
                foreach (var item in MainDifficultySettings)
                {
                    item.PostInitValidate(this, listOfErrors);
                }
            }

            if (OptionSets != null)
            {
                //validate that custom difficulties are present in the main list:
                foreach (var item in OptionSets)
                {
                    foreach (var difficulty in item.OptionsGroupedByDifficulty)
                    {
                        string difficultyKey = difficulty.First().Difficulty.KeyName;

                        if (MainDifficultySettings.FirstOrDefault(d => d.KeyName == difficultyKey) == null)
                        {
                            EntityType.CreateValidationError(ref listOfErrors, "The custom difficulty key: " + difficultyKey + " was not found in the main difficulty list.");
                        }
                    }
                }

            }
            else 
            {
                EntityType.ValidateRequiredValue(ref listOfErrors, "OptionSets", false);
            }

        }

    }
}

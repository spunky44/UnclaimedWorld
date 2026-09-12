using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Scenarios
{
    public class CustomDifficulty
    {
        /// <summary>
        /// must correspond to a difficulty in MainDifficultySettings
        /// </summary>
        public string KeyName;

        /// <summary>
        /// if filled, will be displayed in the difficulty ddl instead of the main difficulty name
        /// </summary>
        public string Name;
       
        public float ScoreModifier;


        public bool IsDefault = false;

    }

}

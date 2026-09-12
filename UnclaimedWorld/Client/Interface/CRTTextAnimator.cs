using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;

namespace GameEngine.Interface
{
    public class CRTTextAnimator
    {
        private List<Label> labels;

        private List<string> originalTexts = new List<string>();

        public float TimeBetweenCharacters = 0.10f;
        private double timePassed = 0f;

        private bool isStarted = false;

        private int currentLabelIndex = 0;

        public void Add(Label label)
        {
            labels.Add(label);
        }

        public void StartAnimating()
        {
            foreach (Label label in labels)
            {
                originalTexts.Add(label.Text);
                label.Text = "";
            }

            isStarted = true;
            timePassed = 0;
            currentLabelIndex = 0;

        }

        public void Update(GameTime gameTime)
        {
            if (isStarted)
            {
                timePassed += gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (timePassed > TimeBetweenCharacters)
            {
                Label currentLabel = labels[currentLabelIndex];
                string orgText = originalTexts[currentLabelIndex];

                int textLength = currentLabel.Text.Length;

                if (textLength == orgText.Length)
                {
                    // go to next label
                    currentLabelIndex++;
                    if (currentLabelIndex == labels.Count)
                    {
                        // all finished.
                        isStarted = false;
                    }
                }
                else 
                {
                    // add a character
                    currentLabel.Text = originalTexts[currentLabelIndex].Substring(0, textLength + 1);
                }

                
                timePassed = timePassed - TimeBetweenCharacters;
            }
        }

    }
}

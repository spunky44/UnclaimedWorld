using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace WindowSystem
{
    public class CRTTextCharAnimator : CRTTextAnimator
    {
        private List<Label> labels = new List<Label>();
        private List<string> originalTexts = new List<string>();

        private string cursorString = "I";


        public CRTTextCharAnimator(GUIManager gui)
            : base(gui)
        {


        }

        public override void Add(Label label)
        {
            labels.Add(label);
        }

        public override void Clear()
        {
            // if we are stopped prematurely, make sure all labels have their full text!!!            
            for (int i = 0; i < labels.Count; i++)
            {
                labels[i].Text = originalTexts[i];
            }

            labels.Clear();
            originalTexts.Clear();

            Stop(); // make sure we're stopped.
        }

        public override int Add(UIComponent control)
        {
            throw (new Exception("Can only add labels..."));
        }

        public override void StartAnimating()
        {
            if (isStarted)
                return;

            if (labels.Count > 0)
            {
                originalTexts.Clear();

                foreach (Label label in labels)
                {
                    originalTexts.Add(label.Text);
                    label.Text = "";
                }

                StartIt();
            }




        }

       


        public override void Update(GameTime gameTime)
        {
            if (labels.Count > 0)
            {
                if (isStarted)
                {
                    timePassed += gameTime.ElapsedGameTime.TotalSeconds;


                    if (timePassed > TimeBetweenUpdates)
                    {

                        Label currentLabel = labels[currentIndex];
                        string orgText = originalTexts[currentIndex];

                        int textLength = currentLabel.Text.Length - cursorString.Length;

                        if (orgText == "" || textLength == orgText.Length)
                        {
                            currentLabel.Text = orgText;

                            // go to next label
                            currentIndex++;
                            if (currentIndex == labels.Count)
                            {
                                Stop();

                            }
                        }
                        else
                        {
                            // add a character
                            currentLabel.Text = orgText.Substring(0, textLength + 1) + cursorString;
                        }


                        timePassed = timePassed - TimeBetweenUpdates;
                    }
                }
            }
        }

    }
}

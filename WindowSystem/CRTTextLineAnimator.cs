using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace WindowSystem
{
    public class CRTTextLineAnimator : CRTTextAnimator
    {        
        
      //  private Dictionary<int, List<Label>> labelsByLineNumber = new Dictionary<int, List<Label>>();
        private SortedDictionary<int, List<Label>> labelsByLineNumber = new SortedDictionary<int, List<Label>>();
        private Dictionary<Label, string> originalTextsByLabel = new Dictionary<Label, string>();
        private int[] lineNumbers;
        
        

        public CRTTextLineAnimator(GUIManager gui)
            : base(gui)
        {


        }

        public override void Add(Label label)
        {
            
            // collect the labels with the same line number:
            List<Label> labelsForThisLineNumber;
            if (labelsByLineNumber.TryGetValue(label.AnimateOnCRTScreenLineNo.Value, out labelsForThisLineNumber))
            {
                labelsForThisLineNumber.Add(label);
            }
            else
            {
                labelsForThisLineNumber = new List<Label>();
                labelsForThisLineNumber.Add(label);
                labelsByLineNumber.Add(label.AnimateOnCRTScreenLineNo.Value, labelsForThisLineNumber);
            }
                
            
        }

        public override void Clear()
        {
            // if we are stopped prematurely, make sure all labels have their full text!!!
            foreach (KeyValuePair<int, List<Label>> kvp in labelsByLineNumber)
            {
                foreach (Label label in kvp.Value)
                {
                    label.Text = originalTextsByLabel[label]; // .Add(label, label.Text);
                }

            }

            labelsByLineNumber.Clear();
            originalTextsByLabel.Clear();

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

            if (labelsByLineNumber.Count > 0)
            {
                originalTextsByLabel.Clear();

                foreach (KeyValuePair<int, List<Label>> kvp in labelsByLineNumber)
                {
                    foreach (Label label in kvp.Value)
                    {                        
                        originalTextsByLabel.Add(label, label.Text);
                        label.Text = "";
                    }
                    
                }

                //  SortedDictionary<int, List<Label>>.KeyCollection keys = labelsByLineNumber.Keys;

                // sorted keys (line numbers, with gaps)
                // use them to look up into the dictionary
                lineNumbers = labelsByLineNumber.Keys.ToArray<int>();

                StartIt();
            }
           

            
        }

              

        public override void Update(GameTime gameTime)
        {
            if (labelsByLineNumber.Count > 0)
            {
                if (isStarted)
                {
                    timePassed += gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (timePassed > TimeBetweenUpdates)
                {        
                    
                    int currentLineNumber = lineNumbers[currentIndex];
                    List<Label> currentLabels = labelsByLineNumber[currentLineNumber]; // labels[currentLabelIndex];

                    foreach (Label label in currentLabels)
                    {   // fill in all labels on this line:
                        label.Text = originalTextsByLabel[label]; //[currentLabelIndex];
                    }
                    
                    // go to next line (of labels)
                    currentIndex++;
                    if (currentIndex == lineNumbers.Length)
                    {
                        Stop();
                    }                   


                    timePassed = timePassed - TimeBetweenUpdates;
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WindowSystem
{
    public class TextArea : ListBox
    {
        #region Fields
        private string text;
        #endregion

        #region Properties

        
        /// <summary>
        /// Get/Set the control text.
        /// </summary>
        public string Text
        {
            get { return this.text; }
            set
            {
               
                this.text = value;
                RefreshText();
            }
        }

        /// <summary>
        /// Get/Set the control width.
        /// NEW: Also re-break lines after a container grid sets the width.
        /// </summary>
        public override int Width
        {
            get { return base.Width; }
            set
            {
                if (base.Width != value)
                {
                    base.Width = value;
                    RefreshText();

                    RefreshMargins(); 
                }
            }
        }

        /*
        public new int Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                RefreshText();

                RefreshMargins(); // NEW
            }
        }*/

        /// <summary>
        /// Get/Set the control height.
        /// </summary>
        public new int Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;

                RefreshMargins(); // NEW

                RefreshText();
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        /// <param name="gui">GUIManager that this control is part of.</param>
        public TextArea(GUIManager gui, ListBoxType type)
            : base(gui, type)
        {
            CanHaveFocus = false;
            this.text = "";
            CanGrowInHeight = true;

        }

        #endregion

        public enum TextAreaType
        {
            HUD
        }

     //   TextAreaType type;

      /*  public void Init(WindowSystem.Label.LabelType type) // TextAreaType type)
        {
            labelType = type;
           // this.type = type;

          //  switch (type)
          //  {
         //       case TextAreaType.HUD:
         //           Font = GUIManager.LCDandHUDFontPath;
         //           Color = Color.White;         
         //           break;
         //   }
       * 
        }*/

      
        /// <summary>
        /// Refreshes the text in the control.
        /// </summary>
        public void RefreshText()
        {
            
            // Clear all entries
            this.Clear();

            // Break the text into lines
            
            // why was it disabled???
            List<string> lines = BreakTextToEntries();
            
            // too simplistic:
         //   string[] lines = text.Split('\n');

            if (lines != null)
            {
                BeginAddingEntries();

                // Add each line as an entry of the ListBox
                foreach (string str in lines)
                {
                    this.AddEntry(str);
                }

                EndAddingEntries();
               // RefreshEntries();
            }
        }

 
        #region Private Methods

        const char tagMarker = '§';

        private void SplitTagFreeBlockIntoWords(string text, List<string> words)
        {
            var newWords = text.Split(' ');

            if (newWords.Length == 1 && newWords[0] == "")
            {

            }

            words.AddRange(newWords);
        }

        private List<string> SplitIntoWords(string text)
        {
            List<string> words = new List<string>();

            int? beginningTagIndex = null;
            int beginningOfBlock = 0;

            bool hasTextBetweenTags = false;

            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];
                if (currentChar == tagMarker)
                {
                    if (beginningTagIndex == null)
                    {
                        if (hasTextBetweenTags)
                        {
                            // new tag begins - handle the text before:
                            int length = i - beginningOfBlock;
                            if (length > 0)
                            {
                                SplitBlock(text, words, beginningOfBlock, length);

                                beginningOfBlock = i; // i + 1;
                            }
                        }
                         
                        beginningTagIndex = i;
                       
                    }
                    else
                    {
                        // tag ends
                        // NEW: include characters after end tag:
                        beginningTagIndex = null;

                        hasTextBetweenTags = false; // we have not encountered raw text yet
                    }
                } 
                else if (beginningTagIndex == null) // raw text, not inside a tag
                {
                    hasTextBetweenTags = true;
                }
            }

            if (beginningOfBlock == 0 ||
                beginningOfBlock < text.Length - 1)
            {
               // string block = text.Substring(beginningOfBlock, text.Length - beginningOfBlock); 
              //  SplitTagFreeBlockIntoWords(block, words);


                SplitBlock(text, words, beginningOfBlock, text.Length - beginningOfBlock);
            }

            return words;            
        }

        private void SplitBlock(string text, List<string> words, int beginningOfBlock, int length)
        {
            string block = text.Substring(beginningOfBlock, length);

            // don't split text inside a tag, skip past the tag:
            int endTagIndex = block.LastIndexOf(tagMarker);
            int noOfWords = words.Count;
            string tagToken = null;
            if (endTagIndex >= 0)
            {
                tagToken = block.Substring(0, endTagIndex + 1);
                block = block.Substring(endTagIndex + 1, block.Length - endTagIndex - 1);
            }

            if (block != "") // don't add empty words
            {
                SplitTagFreeBlockIntoWords(block, words);

                // append, so punctuation etc. following a tag is part of the same word:
                if (endTagIndex >= 0)
                {
                    words[noOfWords] = tagToken + words[noOfWords];
                }
            }
            else
            {
                words.Add(tagToken);
            }


            // append, so punctuation etc. following a tag is part of the same word:
           /* if (endTagIndex >= 0)
            {
                words[noOfWords] = tagToken + words[noOfWords];
            }*/
        }

        private static char[] spaces = new char[] { ' ' };

        /// <summary>
        /// Breaks the Text into several lines with the width of the TextArea.
        /// </summary>
        /// <returns>A list containing all the generated lines.</returns>
        private List<string> BreakTextToEntries()
        {
            if (string.IsNullOrEmpty(Text)) 
            {
                return null;
            }

            // NEW: allow colored text in TextArea (links too??)
            string plainText = Text; // Label.GetPlainText(Text);
          
          //  string[] words = plainText.Split(' ');
            // don't split tags:
            List<string> words = SplitIntoWords(Text);

            List<string> lines = new List<string>();
            string currentLine = "";
            string proposedLine;
            int proposedLineSize = 0;
            int lineHSize = this.Width - this.HMargin * 2 - GapAndScrollBar; // 4;

            string word;
            for (int i = 0; i < words.Count; i++)
            {
                word = words[i];

                if (word.StartsWith("§")) // == "get")
                {

                }

                string appended;
                if (word.StartsWith("\n"))
                {
                    word = word.Replace("\n", "");
                    lines.Add(currentLine); // start a new line

                    if (word != "")
                    {
                        appended = word + " ";
                    }
                    else
                    {
                        appended = word;
                    }

                    proposedLine = appended; // word + " ";     

                    proposedLineSize = Label.GetParsedLineWidth(proposedLine, guiManager, SpriteFont); // we need the width of the nested icons here, but it has not yet been created. so look up the sprite rects
             
                }
                else
                {
                    appended = word + " ";
                    proposedLine = currentLine + appended; // word + " ";    

                    int appendedSize = Label.GetParsedLineWidth(appended, guiManager, SpriteFont);
                    proposedLineSize += appendedSize;
                }
                               

               // proposedLineSize = Label.GetParsedLineWidth(proposedLine, guiManager, SpriteFont); // we need the width of the nested icons here, but it has not yet been created. so look up the sprite rects
             
                if (proposedLineSize < lineHSize || currentLine == "")
                {
                    currentLine = proposedLine;
                }
                else
                {
                    lines.Add(currentLine); // over the limit. start a new line
                    currentLine = word + " ";

                    proposedLineSize = Label.GetParsedLineWidth(currentLine, guiManager, SpriteFont); // 0;
                }

                if (currentLine.TrimEnd(spaces).EndsWith("\n"))
                {
                    currentLine = currentLine.Replace("\n", "");
                    currentLine.TrimEnd(spaces); // delete spaces after line break
                    lines.Add(currentLine); // start a new line
                    currentLine = "";

                    proposedLineSize = 0;
                }
            }
            currentLine = currentLine.TrimEnd(spaces);
            lines.Add(currentLine);

            return lines;
        }




      

        #endregion
    }
}


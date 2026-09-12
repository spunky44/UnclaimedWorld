using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using WindowSystem;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.PropertyPresentation
{
    public class PresentationData
    {
        public string TermTooltip;
        public string Caption;
        public string Term;
        public string IconName;
        public Color? Color;
        public string Key;
        public EntityID? ClickablePropertyEntityID;//if this is null then the property is not clickable
        public PropertyResult? Result;
        public float? NormalizedValue;

        public PresentationData(
            string presentationCaption,
            string presentationTerm,
            string presentationIcon,
            PropertyResult? propertyValue,
            float? normalizedValue,
            string presentationKey,
            EntityID? clickablePropertyEntityID,
            Color? color,
            string termTooltipValue)
        {
            TermTooltip = termTooltipValue;
            Caption = presentationCaption;
            Term = presentationTerm;
            IconName = presentationIcon;
            Result = propertyValue;
            Key = presentationKey;
            ClickablePropertyEntityID = clickablePropertyEntityID;
            Color = color;
            NormalizedValue = normalizedValue;
        }

        public PresentationData()
        {
        }

      
       
      
    }
}

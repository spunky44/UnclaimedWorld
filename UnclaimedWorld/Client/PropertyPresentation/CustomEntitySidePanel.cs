using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide;

namespace UWGame.ClientSide.PropertyPresentation
{
    public class CustomDataPresentation : IGameDataObject
    {
        public string[] PresentationTypeCategoryKeys;

        public PresentationTypeCategory[] PresentationTypeCategories;

        [XmlIgnore]
        public List<PresentationTypeCategory> FinalPresentationTypeCategories;

        public void Initialize()
        {
            FinalPresentationTypeCategories = new List<PresentationTypeCategory>();
            if (PresentationTypeCategoryKeys != null)
            {
                foreach (var item in PresentationTypeCategoryKeys)
	            {
		            FinalPresentationTypeCategories.Add(GameData.Instance.AllPresentationTypeCategories[item]);
                }               
            }
            else
            {
                foreach (var item in PresentationTypeCategories)
                {
                    item.Initialize();
                    FinalPresentationTypeCategories.Add(item);
                }
            }

            foreach (var item in FinalPresentationTypeCategories) // PresentationTypeCategories)
            {
                //item.Initialize();
            }
        }

        public void PostDataCompleteInitialize()
        {

        }
    }
    
}

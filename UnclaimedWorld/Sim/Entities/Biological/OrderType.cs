using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Biological
{
    public class BioOrderType: IGameData
    {
        public SerializableDictionary<string, BioProperty> BioProperties;

        public string Description;



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
        public void PreInitValidate(ref List<string> errors)
        {
           
        }

        public void Initialize()
        {
           
        }

        public void PostInitValidate(ref List<string> errors)
        {
           
        }

        public bool GetBioPropertyValue(BioPropertyType propertyKey, out BioProperty property)
        {
            return BiologicalEntity.GetBioPropertyValue(propertyKey, BioProperties, out property);
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}

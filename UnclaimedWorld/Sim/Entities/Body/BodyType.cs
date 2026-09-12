using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Body
{
  
    public class BodyType : IGameData    
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
       // [XmlArrayItem(typeof(MachineBodyPartType)), XmlArrayItem(typeof(BodyPartType)), XmlArrayItem(typeof(BiologicalBodyPartType))]
        public BodyPartType[] BodyPartTypes; //List<BodyPartType> BodyPartTypes;

        /// <summary>
        /// if defined, will override the normal computed hitpoints from bulk
        /// </summary>
        public float? Hitpoints;


        /// <summary>
        /// Added this for robots...
        /// </summary>
        public float? Bulk;

        public BodyType()
        {
        }

        public BodyType(string keyName)
        {
            this.KeyName = keyName; 

        }

        public override string ToString()
        {
            return KeyName;
        }

        public BodyPartType FindBodyPart(string name)
        {
            BodyPartType found;

            foreach (BodyPartType bodypart in BodyPartTypes)
            {
                found = bodypart.FindBodyPart(name);

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public void PostLoadContentInitialize()
        {
            foreach (BodyPartType bodypart in BodyPartTypes)
            {
                bodypart.PostLoadContentInitialize();
            }
        }

        #region IGameData Members
                

        public void Initialize()
        {
            foreach (BodyPartType bodypart in BodyPartTypes)
            {
                bodypart.Initialize();
            }
        }


        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors) 
        {
           /* foreach (BodyPartType bodypart in BodyPartTypes)
            {
                bodypart.PostInitValidate();
            }*/
        }
        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
        #endregion
    }
}

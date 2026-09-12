using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.ClientSide;
namespace UWGame.SimSide.Entities.Body
{
   
    [XmlInclude(typeof(MachineBodyPartType))] 
    [XmlInclude(typeof(BiologicalBodyPartType))]
    public abstract class BodyPartType
    {
        //public string TypeName;

        // can machine and bio parts be mixed?
        // make 2 classes
        
        /// <summary>
        /// hide model part if destroyed?
        /// </summary>
        public string ModelMesh;

        
        // the entity functions affected by this body part.
        public BodyPartFunction[] Functions;

        public string Name;

        /// <summary>
        /// this is needed for Attack Types serializer to reference the correct objects.
        /// </summary>
        public string BodyKeyName;

      /*  public MachineBodyPartType MachineBodyPartTypeComponent;
        public BiologicalBodyPartType BiologicalBodyPartTypeComponent;
        */
        public string ArmorLayer;

        [XmlIgnore]
        public BodyLayerType ArmorLayerType;

        // children parts      
      //  [XmlArrayItem(typeof(MachineBodyPartType)), XmlArrayItem(typeof(BodyPartType)), XmlArrayItem(typeof(BiologicalBodyPartType))]
        public BodyPartType[] BodyPartTypes;

       // public MachineBodyPartType[] MachineBodyPartTypes;
       // public BiologicalBodyPartType[] BiologicalBodyPartTypes;

        public float HitpointsFraction;

        public float ToHitProfileFront;
        public float ToHitProfileBack;
        public float ToHitProfileLeft;
        public float ToHitProfileRight;
        
        /// <summary>
        /// serializer uses this
        /// </summary>
        public BodyPartType()
        {
           
        }
        //public Dictionary<PresentationTypeCategory, List<PresentationType<BodyPart>>> healthRepresentation;
     /*   public BodyPartType(string bodyKeyName, string name)
        {
          //  this.KeyName = bodyKeyName + "_" + name;
            this.BodyKeyName = bodyKeyName;

            this.Name = name;
        }*/

        public BodyPartType FindBodyPart(string name)
        {
            if (Name == name)
            {
                return this;
            }
            else
            {
                BodyPartType found = BodyPartTypes.First(b => b.Name == name);

                if (found == null)
                {
                    if (BodyPartTypes.Length > 0)
                    {
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
                    else return null;
                }
                else
                {
                    return found;
                }
            }

        }


        public virtual bool IsVital()
        {
            return false;
        }

        public virtual void PostLoadContentInitialize()
        {
            if (ArmorLayerType != null)
            {
                ArmorLayerType.PostLoadContentInitialize();
            }


            if (BodyPartTypes != null)
            {
                foreach (BodyPartType bodypart in BodyPartTypes)
                {
                    bodypart.PostLoadContentInitialize();
                }
            }
        }


        public virtual void Initialize()
        {
            if (ArmorLayer != null)
            {
                ArmorLayerType = GameData.Instance.AllBodyLayerTypes[ArmorLayer];
            }

            if (BodyPartTypes != null)
            {
                foreach (BodyPartType bodypart in BodyPartTypes)
                {
                    bodypart.Initialize();
                }
            }

            if (ToHitProfileBack == 0f && ToHitProfileFront == 0f && ToHitProfileLeft == 0f && ToHitProfileRight == 0f)
            {
                ToHitProfileBack = ToHitProfileFront = ToHitProfileRight = ToHitProfileLeft = 1f;
            }
        }


      /*  public void PostInitValidate(List<string> listOfErrors)
        {
            if (BiologicalBodyPartTypeComponent != null)
            {
                BiologicalBodyPartTypeComponent.PostInitValidate(listOfErrors);
            }

            if (BodyPartTypes != null)
            {
                foreach (BodyPartType bodypart in BodyPartTypes)
                {
                    bodypart.PostInitValidate(listOfErrors);
                }
            }
        }*/



        public override string ToString()
        {
            return Name;
        }

    }
}

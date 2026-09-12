using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Processes
{
   
    /// <summary>
    /// Perhaps add a Key that the Output class can refer to.
    /// Optional inputs should be placed in an array. OR a new class called OptionalInputs that has a tag property instead of a singel type key.
    /// </summary>
    public class Input 
    {      
        /// <summary>
        /// fill in either one type or the Tag
        /// </summary>
        public string Entity;

        /// <summary>
        /// if filled, the input will have options...
        /// </summary>
        public string Tag;

        public InputAmount Amount;

        /// <summary>
        /// default is False
        /// </summary>
        public bool IsConsumed = false;

        /// <summary>
        /// TODO: move this to Output. 
        /// optional - default is the first output item
        /// </summary>
        public string BecomesPartOfProduct;

       // public int Amount;
        

        /// <summary>
        /// set this reference if the part becomes part of that product 
        /// 
        /// by default, this gets set to the first output (if not consumed)
        /// </summary>
        [XmlIgnore]
        public EntityType BecomesPartOfProductType;

        /*
        [XmlAttribute]
        public int Amount;
        */


        [XmlIgnore]
        public EntityType EntityType;

        [XmlIgnore]
        public float StageLength; // will be 1/Amount * ManSecondsOfWorkNeeded


        public Input() { }

        

        public void PostDataCompleteInitialize()
        {
            if (Amount.NoOfItems.HasValue)
            {
                StageLength = 1f / Amount.NoOfItems.Value;
            }
            else
            {
                StageLength = 1F;
            }

            EntityType = GameData.Instance.AllEntityTypes[Entity]; // GameData.Instance.AllItemTypes[Item];

            Amount.Initialize();

            if (BecomesPartOfProduct != null)
            {
                BecomesPartOfProductType = GameData.Instance.AllEntityTypes[BecomesPartOfProduct];
            }
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (Entity != null)
            {
                EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, Entity);
            }

            if (BecomesPartOfProduct != null)
            {
                EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, BecomesPartOfProduct);
            }

        }


        public bool InputIsImmovable()
        {
            return EntityType.IsImmovable();
        }


        public void Validate(ref List<string> errors)
        {
          /*  if (!string.IsNullOrEmpty(Tag) && !string.IsNullOrEmpty(Entity))            
            {
                EntityType.CreateValidationError(ref errors, "Item and Tag cannot both be specified.");              
            }*/
        }
       
    }


    public class SubstanceInput
    {
        public string Substance;

        public float Amount;

    }
}

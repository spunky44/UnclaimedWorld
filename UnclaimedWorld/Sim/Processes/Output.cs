using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using System.Diagnostics;

namespace UWGame.SimSide.Processes
{
    /// <summary>
    /// should define one type of output from the process: a new entity, a substance in an existing (or new) entity, a replenishment container (firewood in campfire)...
    /// can optionally link to an Input via type? IS another way needed, such as an ID/key?
    /// </summary>
    public class Output 
    {
        /// <summary>
        /// this should be optional.
        /// </summary>
        public OutputAmount Amount;

        /// <summary>
        /// this should be optional. if filled, the entity's Progress property will be used to track progress.
        /// </summary>
        public string EntityTypeToCreate;

        [XmlIgnore]
        public EntityType FinalEntityTypeToCreate;

        /// <summary>
        /// fill either the containers or the placement, not both
        /// 
        /// NOTE: if the container has too little room for the output, it will be placed outside.
        /// </summary>
        public string[] ToolContainerTypesToPlaceIn;

        /// <summary>
        /// fill either the containers or the placement, not both
        /// 
        /// NOTE: if the container has to little room for the output, it will be placed outside.
        /// 
        /// MP: this tag is specified on the tool in itemsLoader. NOT in toolsLoader.
        /// Go to the item in itemloader to specify where they will keep this product once its made (if you don't want them to empty the container on the ground)
        /// Also, make sure that the tools specified for ProcessToolSetKey in ToolsLoader are the same as the ones in ToolContainerTagsToPlaceIn, else the product will appear on the ground.
        /// </summary>
        public string[] ToolContainerTagsToPlaceIn;

        public Vector2? RelativePlacement;


        [XmlIgnore]
        public List<EntityType> ToolContainerToPlaceIn = new List<EntityType>();


       

        /// <summary>
        /// TODO: split this flag and make the behaviour explicitly defined. which inputs become parts and so on.
        /// This flag has 3 effects:
        /// 1. for salvage processes: outputs with this flag are permitted not to exist in the original entity as parts (normally a validation will fire if this is the case)
        /// 2. for other processes: for outputs with this flag, this process will not be listed/used as a manufacturing process for that entity type (for instance in info windows, sawdust will not be shown as made via furniture)
        /// the process wil not be added to ProcessYieldsThisOutput.
        /// 3. The output will not be listed in the "Used in" section of the info window either
        /// 
        /// (Perhaps it is better to split these properties???)
        /// </summary>
        public bool IsWasteProduct = false;

        public void PostDataCompleteInitialize()
        {
            FinalEntityTypeToCreate = GameData.Instance.AllEntityTypes[EntityTypeToCreate];

            GameData.ResolveEntityTypeTags(ref ToolContainerToPlaceIn, ToolContainerTagsToPlaceIn, ToolContainerTypesToPlaceIn, GameData.Instance.ToolsByTag);

        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (EntityTypeToCreate != null)
            {
                EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, EntityTypeToCreate);
            }

          /*  if (BecomesPartOfProduct != null)
            {
                EntityType.ValidateKeyExists(ref listOfErrors, BecomesPartOfProduct);
            }*/

        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {

            if (ToolContainerToPlaceIn != null)
            {
                foreach (var tool in ToolContainerToPlaceIn)
                {
                    if (tool.ContainerType == null || !tool.ContainerType.HasOutputStorage)
                    {
                        Entities.EntityType.CreateValidationError(ref listOfErrors, "The output is specified to be placed inside the tool " + tool.KeyName + ", but this tool does not define a production output storage.");
                    }

                }
            }
        }

     /*   public void PostInitValidate(ref List<string> listOfErrors)
        {
            if (ToolContainerToPlaceIn != null)
            {
                foreach (var tool in ToolContainerToPlaceIn)
                {
                    if (tool.ContainerType == null || !tool.ContainerType.HasOutputStorage)
                    {
                        Entities.EntityType.CreateValidationError(ref listOfErrors, "The output is specified to be placed inside the tool " + tool.KeyName + ", but this tool does not define a production output storage.");
                    }
                  
                }
            }
        }*/

       

        /// <summary>
        /// returns the number and bulk of output items based on the total input bulk and/or substance bulks
        /// </summary>
        /// <param name="totalBulkOfInput"></param>
        /// <param name="extractedSubstances"></param>
        /// <param name="noOfItemsToCreate"></param>
        /// <param name="bulkOfEachOutputItem"></param>
        /// <returns></returns>
        public bool GetOutputAmountsToCreate(float totalBulkOfInput, Dictionary<string, float> extractedSubstances, out int noOfItemsToCreate, out float? bulkOfEachOutputItem)
        {         
            bulkOfEachOutputItem = null;

            int? specifiedNoOfItemsToCreate = null;

            // it is possible to set both noOfItems and FractionOfInputBulk to set the bulk on each item (like 3 wings)
            if (Amount.NoOfItems.HasValue)
            {
                // fixed number of products:
                specifiedNoOfItemsToCreate = Amount.NoOfItems.Value;
            }
            

            if (Amount.Bulk != null)
            {
                // the amount and/or bulk of each output depends on the input bulk.

                float totalBulkOfOutputs;
              //  noOfItemsToCreate = 1;

                // number defined by input size
                if (Amount.Bulk.FractionOfInputBulk.HasValue)
                {
                    totalBulkOfOutputs = totalBulkOfInput * Amount.Bulk.FractionOfInputBulk.Value;
                }
                else
                {
                    // get substance bulk:
                    totalBulkOfOutputs = extractedSubstances[Amount.Bulk.InputSubstance];
                }

                if (Common.IsZero(totalBulkOfInput))
                {
                    noOfItemsToCreate = 0;

                    return true; // false;
                }

                if (FinalEntityTypeToCreate.ItemType != null)// ?? should always be defined
                {
                    if (FinalEntityTypeToCreate.ItemType.HasNoMaximumBulk) 
                    {
                        // create one output item with the full bulk, unless the designer has specified a fixed number:                        
                        noOfItemsToCreate = specifiedNoOfItemsToCreate ?? 1;

                        bulkOfEachOutputItem = totalBulkOfOutputs / (float)noOfItemsToCreate;

                    }
                    else
                    {
                        // these types of items have a maximum bulk... if there are fractions, just make each item equal in bulk...                       
                        if (specifiedNoOfItemsToCreate.HasValue)
                        {
                            noOfItemsToCreate = specifiedNoOfItemsToCreate.Value;
                        }
                        else
                        { 
                            // calculate how many items we get out of the input.
                            noOfItemsToCreate = (int)Math.Ceiling(totalBulkOfOutputs / FinalEntityTypeToCreate.ItemType.MaximumBulk.Value);
                        }

                        if (noOfItemsToCreate > 0)
                        {
                            bulkOfEachOutputItem = totalBulkOfOutputs / (float)noOfItemsToCreate;

                            // cap the bulk:
                            bulkOfEachOutputItem = Math.Min(bulkOfEachOutputItem.Value, FinalEntityTypeToCreate.ItemType.MaximumBulk.Value);
                        }
                    } 
                    
                    return true;
                }

                Debug.Assert(false, "This Should Never Happen. Data Error? We should look into this problem.");               
            }

            if (specifiedNoOfItemsToCreate.HasValue)
            {
                noOfItemsToCreate = specifiedNoOfItemsToCreate.Value;
                return true;
            }

            Debug.Assert(false, "This Should Never Happen. Data Error? We should look into this problem.");

            noOfItemsToCreate = 0;
            return false; // return false if the data is wrong??? should never happen - we should validate this.
           
        }


        public Entity GetToolContainerToPlaceOutputIn(List<EntityID> tools)
        {
            if (ToolContainerToPlaceIn != null)
            {
                if (tools != null)
                {
                    foreach (var item in tools)
                    {
                        Entity tool = Entity.FindByID(item);
                        if (tool != null && ToolContainerToPlaceIn.Contains(tool.EntityType))
                        {
                            return tool;
                        }
                    }
                }
            }

            return null;
        }


        /*  public System.Xml.Schema.XmlSchema GetSchema()
          {
              return null;
          }

          public void ReadXml(System.Xml.XmlReader reader)
          {
              CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
          }

          public void WriteXml(System.Xml.XmlWriter writer)
          {
              CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
          }

          public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(Output))
          {
              TypeMappings = EntityType.GetListOfTypeMappings()
                        
          };*/
    }
}

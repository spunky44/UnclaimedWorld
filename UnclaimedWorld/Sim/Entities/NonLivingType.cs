using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using System.Xml.Serialization;
using UWGame.SimSide.Processes;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.Entities
{
    public class NonLivingType //: IXmlSerializable
    {
       
      
        /// <summary>      
        /// on leaf items, degrades Condition
        /// on composite items, this determines degrade of Integrity
        /// (composites get an average of the condition of their parts and Integrity with a weight determined by IntegrityWeightInCondition)
        /// </summary>
        public string DegradeType;

        public DegradeType FinalDegradeType;


        /// <summary>
        /// if true, the parts making up this composite item have protection from the elements
        /// </summary>
        public bool PartsAreWeatherProof { get; set; }


        
        public string DegradesTo;

        [XmlIgnore]
        public EntityType DegradesToType;


        /// <summary>
        /// true if this is ever a part
        /// </summary>
        [XmlIgnore]
        public bool CanBeAPart;


        /// <summary>
        /// 0 - 1
        /// describes how much Integrity matters when computing the condition of composite (=having parts) items
        /// 
        /// 
        /// if set to 0, integrity will not have an influence on when this item breaks (integrity is disabled).
        /// 
        /// not sure if the other values matter right now. There is only a Sim effect when integrity reaches 0. The combined Condition is mostly (only?) for feedback.
        /// </summary>
        public float IntegrityWeightInCondition = 0.3f;


        public string SalvageProcess;

        [XmlIgnore]
        public ProcessType SalvageProcessType;

        /// <summary>
        /// 0 - 1
        /// if less than 1, makes maxCondition decrease when taking condition damage
        /// don't move this to repairType.
        /// </summary>
        public float Repairability;

        public string Repair;

        [XmlIgnore]
        public RepairProfile RepairProfile;

        /// <summary>
        /// contains the generated process types
        /// </summary>
        [XmlIgnore]
        public EntityRepairProfile EntityRepairProfile;


        /// <summary>
        /// TODO: move to NonLivingEntityType
        /// the number of parts that the item is composed of - production may also consume other materials though. See MaterialInput.
        /// </summary>
        [XmlIgnore]
        public Dictionary<EntityType, int> Parts;

        [XmlElement("Parts")]
        public SerializableDictionary<string, int> PartKeys;


        /// <summary>
        /// a numbering for this entity's
        /// </summary>
        [XmlIgnore]
        public int PartID;

        public void Initialize()
        {


        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {           
            if (PartKeys != null)
            {
                foreach (var item in PartKeys)
                {
                    EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, item.Key);
                }
            }

            if (DegradesTo != null)
            {
                EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, DegradesTo);           
            }
        }

        public void PostDataCompleteInitialize(EntityType parent)
        {
            if (DegradesTo != null)
            {
                DegradesToType = GameData.Instance.AllEntityTypes[DegradesTo];
            }

            if (DegradeType != null)
            {
                FinalDegradeType = GameData.Instance.AllDegradeTypes[DegradeType];
            }

            if (Repair != null)
            {
                RepairProfile = GameData.Instance.AllRepairProfiles[Repair];

                EntityRepairProfile = new EntityRepairProfile();

               // EntityRepairProfile.GenerateProcesses(parent); // done after process graph is created
            }

            if (PartKeys != null)
            {
                Parts = new Dictionary<EntityType, int>();
                foreach (var part in PartKeys)
                {                    
                    EntityType partType = GameData.Instance.AllEntityTypes[part.Key];
                    Parts.Add(partType, part.Value);

                    if (partType.NonLivingType != null)
                    {
                        partType.NonLivingType.CanBeAPart = true;
                    }
                }
            }
        }

        
        /// <summary>
        /// cache this result if it is called regularly
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public List<StorageDuration> GetRepresentativeStorageConditions(EntityType parent)
        {
            
            if (FinalDegradeType != null)
            {
                List<StorageDuration> durations = null;

                // score the durations by relevance. remove ones with same duration...
                float maxDuration = 0f;
                foreach (var thisDuration in FinalDegradeType.StorageDurations)
                {
                    if (thisDuration.Duration > maxDuration)
                    {
                        maxDuration = thisDuration.Duration;
                    }

                    if (thisDuration.StorageDurationToDisplay.DisplayThis(parent))
                    {
                        Common.AddToList(ref durations, thisDuration);
                    }
                }


                /* foreach (var item in GameData.Instance.GUIConstants.StorageDurationToDisplay)
                 {                               
                     StorageCondition condition = GameData.Instance.AllStorageConditions[item.StorageCondition];

                     List<StorageDuration> thisDurations;
                     FinalDegradeType.StorageDurations.TryGetValue(condition, out thisDurations); // are grouped on condition

                     foreach (var thisDuration in thisDurations)
                     {
                         if (thisDuration.StorageDurationToDisplay == item) // only handle the matching durations...
                         {
                             if (thisDuration.StorageDurationToDisplay.DisplayThis(parent))
                             {
                                 Common.AddToList(ref durations, thisDuration);
                             }
                         }
                     }                   
                
                 }*/


                // sort, filter & cap:
                // var ordered = durations.OrderBy(d => d.StorageDurationToDisplay.Priority).ToList();
                //  var ordered = durations.OrderByDescending(d => (d.StorageDurationToDisplay.DisplayAlways? 10000f : 1f) * d.Duration).ToList();
                var ordered = durations.OrderByDescending(d => d.SortOrder).ToList();

                while (ordered.Count > 4)
                {
                    ordered.RemoveAt(ordered.Count - 1);
                }


                return ordered;
            }

            return null;
        }


        public ProcessType SelectRepairProcess(RepairAction action, EntityType part) //EntityID? partToFix)
        {
            if (action == RepairAction.Integrity)
            {
                return EntityRepairProfile.Integrity; 
            }
            else if (action == RepairAction.Condition)
            {
                return EntityRepairProfile.Condition;
            }
            else if (action == RepairAction.PartsCondition)
            {
                return EntityRepairProfile.PartsCondition[part]; 
            }
            else if (RepairType.IsReplaceAction(action))
            {
                return EntityRepairProfile.PartsReplacement[part];
            }

            return null;
        }

        public void PostLoadContentInitialize()
        {
           
            if (!string.IsNullOrEmpty(SalvageProcess))
            {
                // can this be avoided? designers often forget to set this field.
                SalvageProcessType = GameData.Instance.AllProcessTypes[SalvageProcess];

                SalvageProcessType.SetIsSalvageProcess();
                                
            }

        }

        /*
        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(NonLivingType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true)

        };

        #endregion*/
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.RepairTypes
{
   
    /// <summary>
    /// this class is shared between EntityTypes. Each EntityType needs an instance of a different class to hold references to its generated ProcessTypes.
    /// 
    /// it should be possible to repair nested parts... changing a gasket inside an engine in a vehicle for instance...
    /// </summary>
    public class RepairProfile: IGameData
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

        public string Comments;


        /// <summary>
        /// used for increasing integrity
        /// 
        /// not on nested parts
        /// </summary>
        public RepairType Integrity;

        /// <summary>
        /// used for reconditioning the entity when not a part
        /// </summary>
        public RepairType Condition;
        
        /// <summary>
        /// used for replacing all parts that do not have a process specified in PartsReplacement
        /// </summary>
        public RepairType DefaultPartsReplacement;

        /// <summary>
        /// used for reconditioning all parts that do not have a process specified in PartsCondition
        /// </summary>
        public RepairType DefaultPartsCondition;

        /// <summary>
        /// overrides DefaultPartsReplacement when replacing parts 
        /// - can also refer to nested parts!
        /// 
        /// </summary>
        public SerializableDictionary<string, RepairType> PartsReplacement;
        
        [XmlIgnore]
        public Dictionary<EntityType, RepairType> PartsReplacementFinal;


        /// <summary>
        /// overrides DefaultPartsCondition when increasing condition on parts
        /// - can also refer to nested parts!
        /// 
        /// </summary>
        public SerializableDictionary<string, RepairType> PartsCondition;

        [XmlIgnore]
        public Dictionary<EntityType, RepairType> PartsConditionFinal;


        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) 
        {
            if (Integrity != null)
            {
                Integrity.PreDataCompleteValidate(ref listOfErrors);
            }

            if (DefaultPartsCondition != null)
            {
                DefaultPartsCondition.PreDataCompleteValidate(ref listOfErrors);
            }

            if (DefaultPartsReplacement != null)
            {
                DefaultPartsReplacement.PreDataCompleteValidate(ref listOfErrors);
            }
        
        }

        public void PostDataCompleteInitialize()
        {

            if (Integrity != null)
            {
                Integrity.PostDataCompleteInitialize();
            }

            if (DefaultPartsCondition != null)
            {
                DefaultPartsCondition.PostDataCompleteInitialize();
            }

            if (DefaultPartsReplacement != null)
            {
                DefaultPartsReplacement.PostDataCompleteInitialize();
            }

            if (PartsReplacement != null)
            {
                PartsReplacementFinal = new Dictionary<EntityType, RepairType>();
                foreach (var item in PartsReplacement)
                {
                    PartsReplacementFinal.Add(GameData.Instance.AllEntityTypes[item.Key], item.Value);
                }
            }

            if (PartsCondition != null)
            {
                PartsConditionFinal = new Dictionary<EntityType, RepairType>();
                foreach (var item in PartsCondition)
                {
                    PartsConditionFinal.Add(GameData.Instance.AllEntityTypes[item.Key], item.Value);
                }
            }

            if (PartsConditionFinal != null)
            {
                foreach (var item in PartsConditionFinal)
                {
                    item.Value.PostDataCompleteInitialize();
                }
            }

            if (PartsReplacementFinal != null)
            {
                foreach (var item in PartsReplacementFinal)
                {
                    item.Value.PostDataCompleteInitialize();
                }
            }
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {



        }
    }
}

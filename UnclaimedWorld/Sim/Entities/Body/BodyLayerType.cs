using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Entities.Body
{
    [DebuggerDisplay("{KeyName}")]
    public class BodyLayerType: IGameData
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

        
        /// <summary>
        /// the relative thickness of this layer to the other layers
        /// </summary>
        public float Thickness;

        /// <summary>
        /// the resistance of this layer against various types of damage
        /// per thickness unit?
        /// high to simulate resistance against non-penetrating attacks (blunt damage)
        /// </summary>
        public Dictionary<string, float> DamageReductionFactor;

        [XmlIgnore]
        public Dictionary<DamageType, float> DamageReductionFactorFinal;


        /// <summary>
        /// this amount is always deducted from an attack. Can nullify attacks with a damage lower than this number.
        /// high to simulate resistance against penetrating attacks (piercing)
        /// </summary>
        public Dictionary<string, float> DamageReductionConstant;

        [XmlIgnore]
        public Dictionary<DamageType, float> DamageReductionConstantFinal;


        public override string ToString()
        {
            return Name;
        }


        public void PostLoadContentInitialize()
        {
            DamageReductionConstantFinal = new Dictionary<DamageType, float>();
            foreach (var item in DamageReductionConstant)
            {
                DamageReductionConstantFinal.Add(GameData.Instance.AllDamageTypes[item.Key], item.Value);                
            }


            DamageReductionFactorFinal = new Dictionary<DamageType, float>();
            foreach (var item in DamageReductionFactor)
            {
                DamageReductionFactorFinal.Add(GameData.Instance.AllDamageTypes[item.Key], item.Value);
            }
        }


        #region IGameData Members


        public void Initialize()
        {
           
        }


        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors)
        {          
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

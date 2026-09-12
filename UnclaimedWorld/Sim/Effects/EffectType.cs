using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.SimEffects
{
    
   
   

    public enum Falloff { None, Linear, Lerp }

    public enum AIDesirability { Low, Average, High, Highest }

   
    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    [XmlInclude(typeof(NumberEffectType))]
    [XmlInclude(typeof(FlagEffectType))]
    public class EffectType: IGameData
    {
        public string KeyName { get; set; }

        /// <summary>
        /// not very useful.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// make duration same or larger than NeedType.DecreasePerDay to ensure that the agent will try to have the effect always active...
        /// </summary>
        public double? DurationInDays;

        public EvalNode DynamicDurationInDays;

       // public EffectOperator Operator;

      

        public AIDesirability AIDesirability;

        public Falloff Falloff;

        
       

        /// <summary>
        /// for NeedTypes, DetectionFactors, AttackTypes etc.
        /// </summary>
        public string[] AffectsTypeKey;
        public string[] AffectsTypeTag;

       
        public virtual float? GetIntensity()
        {
            return null;
        }

        public enum Background { White, EntityTypeTooltipGreen }
        public virtual void AppendAsString(StringBuilder text, Background background)
        {
            
        }

        public bool DeleteRecord
        {
            get;
            set;
        }

        public void Initialize()
        {
        }

        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors)
        { }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

    }
}

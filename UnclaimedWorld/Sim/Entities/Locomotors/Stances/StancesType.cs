using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities.Locomotors.Stances
{
    public class StancesType: IGameData
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

        public string[] Stances;

        [XmlIgnore]
        public List<StanceType> StanceTypes;
        
        public ChanceToTakeStance[] IdleStancesNearHome;
      
        public ChanceToTakeStance[] IdleStancesAwayFromHome;


        public ChanceToTakeStance[] PatrolStancesLongerWait;
        public ChanceToTakeStance[] PatrolStancesBriefWait;

        public ChanceToTakeStance[] SleepStances;

        public ChanceToTakeStance[] SearchStancesLongerWait;
        public ChanceToTakeStance[] SearchStancesBriefWait;


        /// <summary>
        /// to be used when no more specific stances have been specified. Default stance should specify any modifiers.
        /// </summary>
        public string DefaultStance;

        [XmlIgnore]
        public StanceType DefaultStanceType;

        /// <summary>
        /// to be used when the process type does not define any stances
        /// </summary>
        public string DefaultStanceWhenWorking;

        [XmlIgnore]
        public StanceType DefaultStanceTypeWhenWorking;
        

        public string MovingStance;

        [XmlIgnore]
        public StanceType MovingStanceType;

        public string IncapacitatedStance;

        [XmlIgnore]
        public StanceType IncapacitatedStanceType;

        

        /// <summary>
        /// from, to, duration
        /// 
        /// Note: If these are not defined, no stance change animation will play! Instead, normal animation blending will be used.
        /// </summary>
        public StanceChangeDuration[] StanceChangeDurations;
        
        [XmlIgnore]
        public Dictionary<StanceType, Dictionary<StanceType, double>> StanceChangeDurationsMapping;


        public class StanceChangeDuration
        {
            public string FromStance;

            [XmlIgnore]
            public StanceType FromStanceType;

            public string ToStance;

            [XmlIgnore]
            public StanceType ToStanceType;

            public double Duration;


            public void Initialize()
            {
                FromStanceType = GameData.Instance.AllStanceTypes[FromStance];
                ToStanceType = GameData.Instance.AllStanceTypes[ToStance];
            }

        }


        //  public float IdleChanceToTalk;
        public float IdleChanceToSitFactor;
        public float IdleChanceToKneelFactor;
        public float IdleChanceToStandFactor;
        public float IdleChanceToLayDownFactor;
        public float IdleRemainInCurrentSitOrStandStanceAddend;




        public void PreInitValidate(ref List<string> errors)
        {
           
        }

        


        public void Initialize()
        {
           
            foreach (var item in Stances)
            {
                Common.AddToList(ref StanceTypes, 
                    GameData.Instance.AllStanceTypes[item]);               
            }

            InitStances(IdleStancesNearHome);
            InitStances(IdleStancesAwayFromHome);

            InitStances(PatrolStancesBriefWait);
            InitStances(PatrolStancesLongerWait);

            InitStances(SearchStancesBriefWait);
            InitStances(SearchStancesLongerWait);

            InitStances(SleepStances);

            DefaultStanceTypeWhenWorking = GameData.Instance.AllStanceTypes[DefaultStanceWhenWorking];
            DefaultStanceType = GameData.Instance.AllStanceTypes[DefaultStance];
            MovingStanceType = GameData.Instance.AllStanceTypes[MovingStance];
            IncapacitatedStanceType = GameData.Instance.AllStanceTypes[IncapacitatedStance];

            if (StanceChangeDurations != null)
            {
                StanceChangeDurationsMapping = new Dictionary<StanceType, Dictionary<StanceType, double>>();

                foreach (var item in StanceChangeDurations)
                {
                    item.Initialize();
                }

                foreach (var item in StanceChangeDurations)
                {
                    Dictionary<StanceType, double> innerDict;

                    if (!StanceChangeDurationsMapping.TryGetValue(item.FromStanceType, out innerDict))
                    {
                        innerDict = new Dictionary<StanceType, double>();
                        Common.AddToDictionary(ref StanceChangeDurationsMapping,
                            item.FromStanceType, innerDict);

                        foreach (var item2 in StanceChangeDurations)
                        {
                            if (item2.FromStance == item.FromStance
                                && !innerDict.ContainsKey(item2.ToStanceType))
                            {
                                innerDict.Add(item2.ToStanceType, item2.Duration);
                            }
                        }
                    }                    
                }

            }
        }

        private void InitStances(ChanceToTakeStance[] stances)
        {
            if (stances != null)
            {
                foreach (var item in stances)
                {
                    item.Initialize();
                }
            }

        }

        public void PostInitValidate(ref List<string> errors)
        {
            if (!StanceTypes.Exists(s => s.AnimModifier == null))
            {
                EntityType.CreateValidationError(ref errors, "There must be one default stance in Stances that does not define an AnimModifier flag.");

            }
           
        }


        public bool IsDefaultStance(AnimConditions conditionSet)
        {
            if (conditionSet == null
               || conditionSet.Modifiers == null)
            {
                return true;
            }
            else
            {
                // test that no stance flags are currently set
                return !StanceTypes.Exists(s => s.AnimModifier.HasValue && conditionSet.Modifiers.Test(s.AnimModifier.Value));
            }

            /*
            return conditionSet == null
                || conditionSet.Modifiers == null // test that no stance flags are currently set
                || !(conditionSet.Modifiers.Test(AnimModifier.Sitting)
                        || conditionSet.Modifiers.Test(AnimModifier.Kneeling)
                        || conditionSet.Modifiers.Test(AnimModifier.Lying));*/
        }

        /// <summary>
        /// if it is a ChangeStance action, returns the resulting end stance
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public AnimModifier? GetEndStance(AnimConditionInfo info)
        {

            if (info.ConditionSet.Modifiers == null)
            {
                // means standing/default...
                return null;
            }
            else
            {
                BitMask64 modifiers = info.ConditionSet.Modifiers;

                /*                 
                SoundAndAnimationSet = new RandomSoundAndAnimationSet(){ BaseAnimations = new string[]{  "kneelToSitting" }}, // sitting from kneeling
                ConditionSet = new AnimConditions(){ Action = AnimAction.ChangingStance,  
                                                    Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Sitting, (int)AnimModifier.Kneeling, (int)AnimModifier.Reverse)}, //default direction is "get up" so, sitting down needs "reverse flag"
                                             
                 */

                // get the sim stances from the modifier flags... yikes..
               // StanceType matchingStanceType = StanceTypes.FirstOrDefault(s => info.ConditionSet.Modifiers.Test(s.AnimModifier.Value));
                List<StanceType> matchingStanceTypes = StanceTypes.FindAll(s => s.AnimModifier.HasValue && info.ConditionSet.Modifiers.Test(s.AnimModifier.Value));

                if (matchingStanceTypes == null)
                    return null; // also means standing/default...


                int minNumber = matchingStanceTypes.Min(s => s.Number);
                int maxNumber = matchingStanceTypes.Max(s => s.Number);

                /*
                bool kneelingIsSet = modifiers.Test(AnimModifier.Kneeling);
                bool sittingIsSet = modifiers.Test(AnimModifier.Sitting);
                bool lyingIsSet = modifiers.Test(AnimModifier.Lying);
                */
                
                if (modifiers.Test(AnimModifier.Reverse))
                {    
                    // getting down...

                    return matchingStanceTypes.First(s => s.Number == minNumber).AnimModifier; // matchingStanceType.AnimModifier;

                    /*
                    if (lyingIsSet)
                    {
                        return AnimModifier.Lying;
                    }
                    else if (sittingIsSet)
                    {
                        return AnimModifier.Sitting;
                    }
                    else if (kneelingIsSet)
                    {
                        return AnimModifier.Kneeling;
                    }*/

                }
                else
                {
                    // getting up...
                    if (matchingStanceTypes.Count > 1)
                    {
                        return matchingStanceTypes.First(s => s.Number == maxNumber).AnimModifier;
                    }
                    else
                    {
                        // standing up. Standing has null as AnimModifier.:
                        return null;
                    }

                    /*
                    
                    if (lyingIsSet)
                    {
                        if (sittingIsSet)
                        {
                            return AnimModifier.Sitting;
                        }
                        else if (kneelingIsSet)
                        {
                            return AnimModifier.Kneeling;
                        }
                        else
                        {
                            return null; // -> stand
                        }

                    }
                    else if (sittingIsSet)
                    {
                        if (kneelingIsSet)
                        {
                            return AnimModifier.Kneeling;
                        }
                        else
                        {
                            return null; // -> stand
                        }
                    }
                    else if (kneelingIsSet)
                    {
                        return null; // -> stand
                    }*/

                }

            }



            return null;
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }

    public class ChanceToTakeStance
    {
        /// <summary>
        /// leaving this out gives equal chance 
        /// </summary>
        public float? Chance;
        public float? AddedChanceToRemainInStance;

        public string Stance;

        [XmlIgnore]
        public StanceType StanceType;


        public void Initialize()
        {
            StanceType = GameData.Instance.AllStanceTypes[Stance];

        }
    }
}

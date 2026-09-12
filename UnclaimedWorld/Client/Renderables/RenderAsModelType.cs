using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using System.Collections;
using System.Diagnostics;
using UWGame.Client.Particles;
using Xclna.Xna.Animation;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using Microsoft.Xna.Framework.Content;

namespace UWGame.ClientSide.Renderables
{

    public class RenderAsModelType
    {
        /// <summary>
        /// these are overridden by BiologicalType
        /// </summary>
        public float ModelScale = 1f;
        public string ModelBasicTextureName;


        public XmlDictionary<string, GaitAnimationBracket[]> GaitAnimations;

        public RenderAsModelType()
        {


        }

        public void LoadContent(ContentManager content)
        {
         /*   if (this.AnimConditions != null)
            {
                foreach (var item in AnimConditions)
                {
                    item.LoadContent(content);
                }
            }*/
        }

        public void Initialize()
        {
              
            if (AnimConditions != null)
            {
                if (this.AssetName == "man")
                {

                }

                AnimConditionsByAction = new Dictionary<AnimAction, List<AnimConditionInfo>>();

                foreach (var item in AnimConditions)
                {
                    item.Initialize();

                    if (item.ConditionSet.Action.HasValue)
                    {
                        List<AnimConditionInfo> infos;
                        if (!this.AnimConditionsByAction.TryGetValue(item.ConditionSet.Action.Value, out infos))
                        {
                            infos = new List<AnimConditionInfo>();
                            AnimConditionsByAction.Add(item.ConditionSet.Action.Value, infos);
                        }

                        infos.Add(item);
                    }
                }

            }

            //here we let each RenderAsModelType decide which of its conditions is best as default         
            if (DefaultInfo == null)
            {
                AnimConditions defaultMask = new AnimConditions() { Action = AnimAction.Idle, Modifiers = new BitMask64() }; // Modifiers new BitMask64(typeof(Modifier), (int)Modifier.Idle);
                AnimConditionInfo defaultInfo = new AnimConditionInfo();

                FindBestAnimInfo(defaultMask, out defaultInfo);
                DefaultInfo = defaultInfo;
            }


            if (GaitAnimations != null)
            {
                foreach (var item in GaitAnimations)
                {
                    item.Value.Initialize();
                }
            }
        }


        public void PostLoadContentValidate(ref List<string> listOfErrors, EntityType parent) // RenderableType parent)
        {
            // get all possible models
            HashSet<string> allModelNames = new HashSet<string>();
            if (AssetName != null)
            {
                allModelNames.Add(AssetName);
            }

            if (parent != null && parent.BiologicalType != null)
            {
                foreach (var caste in parent.BiologicalType.Castes)
                {
                    foreach (var age in caste.AgeGroupTypes)
                    {
                        if (parent.BiologicalType.RaceTypes != null)
                        {
                            foreach (var race in parent.BiologicalType.RaceTypes)
                            {
                                string modelName = parent.BiologicalType.GetModelName(caste, race, age.Edge);
                                if (modelName != null)
                                {
                                    allModelNames.Add(modelName);
                                }
                            }
                        }
                        else
                        {
                            string modelName = parent.BiologicalType.GetModelName(caste, null, age.Edge);
                            if (modelName != null)
                            {
                                allModelNames.Add(modelName);
                            }
                        }
                    }
                }
                               
            }

            // validate no duplicate anim conditions
            if (AnimConditions != null)
            {
                ValidateConditionSet(AnimConditions, "AnimConditions", ref listOfErrors);
            }

            if (DefaultStances != null)
            {
                ValidateConditionSet(DefaultStances, "DefaultStances", ref listOfErrors);
            }
           

            // validate the animation keys
            foreach (var modelName in allModelNames)
            {
                
                ModelData model = GameData.Instance.AllModels[modelName];

                AnimationInfoCollection animations = AnimationInfoCollection.FromModel(model.Model);

                if (GaitAnimations != null)
                {
                    foreach (var item in GaitAnimations)
                    {
                        foreach (var anim in item.Value)
                        {
                            listOfErrors = ValidateThatAnimExists(listOfErrors, anim.AnimationKey, modelName, animations);
                        }
                    }
                }

                if (DefaultInfo != null)
                {
                    DefaultInfo.PostLoadContentValidate(listOfErrors, modelName, animations);
                }

                if (DefaultStances != null)
                {
                    foreach (var item in DefaultStances)
                    {
                        item.PostLoadContentValidate(listOfErrors, modelName, animations);
                    }
                }

                if (AnimConditions != null)
                {
                    foreach (var item in AnimConditions)
                    {
                        item.PostLoadContentValidate(listOfErrors, modelName, animations);                        
                    }
                }
            }

            
        }

        private static void ValidateConditionSet(AnimConditionInfo[] conditions, string fieldName, ref List<string> listOfErrors)
        {
            int numberOfDistinct = conditions.Distinct().Count();
            if (conditions.Length != numberOfDistinct)
            {
                EntityType.CreateValidationError(ref listOfErrors, "Duplicates found in " + fieldName + ".");
            }         
        }

        public static List<string> ValidateThatAnimExists(List<string> listOfErrors, string key, string modelName, AnimationInfoCollection animations)
        {
            if (!animations.ContainsKey(key))
            {
                EntityType.CreateValidationError(ref listOfErrors, key + " animation not found on model: " + modelName);
            }
            return listOfErrors;
        }

       

        /// <summary>
        /// this model name gets overridden sometimes
        /// </summary>
        public string AssetName
        {
            get;
            set;
        }

       
        public AnimConditionInfo[] AnimConditions;

        [XmlIgnore]
        public Dictionary<AnimAction, List<AnimConditionInfo>> AnimConditionsByAction;

        public AnimConditionInfo DefaultInfo
        {
            get;
            set;
        }

        public AnimConditionInfo[] DefaultStances
        {
            get;
            set;
        }



        //-------------------------------------------------------------------------------------------------
        public void FindBestAnimInfo(AnimConditions condition, out AnimConditionInfo bestMatch)
        {

            // Search through priority best match first

            bestMatch = DefaultInfo; //if we don't find a good match below, then use the defaultiest one we got (could be null)

            if (AnimConditions == null)
                return;

            if (condition.Action == null)
            {
               
                // if no action is specified, we may be in between goals... select the most appropriate idle anim for the stance:
                if (DefaultStances != null)
                {
                    bestMatch = SelectDefaultStance(condition) ?? DefaultInfo;
                }

                return;
            }
            

            int bestScore = 0;// can't be bested by any state with zero matches, so this is default score

            List<AnimConditionInfo> infos;

            if (AnimConditionsByAction.TryGetValue(condition.Action.Value, out infos))
            {
                foreach (AnimConditionInfo matchCandidate in infos)
                {
                    if (matchCandidate.ConditionSet == null) 
                        continue; // empty set doesn't match anything.

                    if (IsSame(condition, matchCandidate))
                    {
                        bestMatch = matchCandidate;
                        return;// perfect match always wins
                    }

                    //score how many bits this info got right 
                    // subtract those that do not match
                    //int matchingBits = 0;
                    //int unmatchingBits = 0;
                    bool isForbidden;

                    //-- Condition states choose the match with the closest match among the "Condition" bits in the 
                    //info record, based on satisfying the most of the "required" conditions and none of the forbidden conditions
                    int modifierScore = ScoreModifiers(condition, matchCandidate, out isForbidden); //, out matchingBits, out unmatchingBits);

                    if (isForbidden)
                        continue;                    

                   
                    int scoreThisInfo = modifierScore + 3; // add x points for correect action state
                    
                    if (scoreThisInfo > bestScore) 
                    {
                        bestScore = scoreThisInfo;
                        bestMatch = matchCandidate;
                    }

                }
            }



        }

        private bool IsSame(AnimConditions condition, AnimConditionInfo matchCandidate)
        {
            if (condition.Action == matchCandidate.GetAction() // can be null
                        && (((condition.Modifiers == null || condition.Modifiers.Bits == 0) && matchCandidate.GetModifiers() == null)
                            || (condition.Modifiers.Bits != 0 && matchCandidate.GetModifiers() != null && condition.Modifiers.Equals(matchCandidate.GetModifiers()))))
            {
                return true;
            }

            return false;
        }

        private AnimConditionInfo SelectDefaultStance(AnimConditions condition)
        {

            int bestScore = 0;
            AnimConditionInfo bestMatch = null;

            DefaultStances.FirstOrDefault(s => IsSame(condition, s));

            foreach (AnimConditionInfo matchCandidate in DefaultStances)
            {

                if (IsSame(condition, matchCandidate))
                {
                    return matchCandidate;
                }
               
                bool isForbidden;
                int score = ScoreModifiers(condition, matchCandidate, out isForbidden);

                if (!isForbidden && score > bestScore)
                {
                    bestScore = score;
                    bestMatch = matchCandidate;
                }
            }

            return bestMatch;
        }

        private int ScoreModifiers(AnimConditions condition, AnimConditionInfo matchCandidate, out bool forbidden) //, out int matchingBits, out int unmatchingBits)
        {
            forbidden = false;
            int matchingBits, unmatchingBits;

            matchingBits = 0;
            unmatchingBits = 0;

            BitMask64 candidateModifiers = matchCandidate.GetModifiers();

            if (condition.Modifiers.Bits != 0) // != null)
            {
                if (matchCandidate.Forbiddens != null)
                {
                    if ((int)condition.Modifiers.CountIntersection(matchCandidate.Forbiddens) > 0)
                    {
                        forbidden = true;
                        return 0;
                       // continue; //forbiddens are absolutely forbidden
                    }
                }

                if (candidateModifiers != null)
                {
                    matchingBits = (int)condition.Modifiers.CountIntersection(candidateModifiers);

                    unmatchingBits = (int)condition.Modifiers.CountInverseIntersection(candidateModifiers);
                }
                else
                {
                    unmatchingBits = (int)condition.Modifiers.CountBits();
                }

            }
            else
            {
                if (candidateModifiers != null)
                {
                    unmatchingBits = (int)candidateModifiers.CountBits();
                }
            }

            return matchingBits - unmatchingBits;
        }
        
        //end findBestAnimInfo
        /*
        public void FindBestAnimInfo(ref BitMask64 c, out AnimConditionInfo best)
        {

            // Search through priority best match first

            best = DefaultInfo; //if we don't find a good match below, then use the defaultiest one we got (could be null)

            if (AnimConditions == null)
                return;

            int bestScore = 0;// can't be bested by any state with zero matches, so this is default score

            AnimState actionState = c.TestForAny;

            foreach (AnimConditionInfo info in AnimConditions)
            {
                if (info.Conditions == null || !info.Conditions.Any())
                    continue; // empty set doesn't match anything.

                // TODO this deserves an Equals(other) method
                if (c.Equals(ref info.Conditions)) // && (info.Forbiddens == null || !info.Forbiddens.Any()))//no forbiddens specified - Lars: Why??
                {
                    best = info;
                    return;// perfect match always wins
                }

                //score how many bits this info got right 
                //Lars: I am tempted to score Action bits higher than Modifier bits...
                int positivesScore = (int)c.CountIntersection(info.Conditions);

                int actionStateScore = 0;
              //  RenderableType.ActionConditionStates

                int negativesScore = (info.Forbiddens == null) ? 0 : (int)c.CountIntersection(info.Forbiddens);

                if (negativesScore > 0)
                    continue; //forbiddens are absolutely forbidden

                //-- Condition states choose the match with the closest match among the "Condition" bits in the 
                //info record, based on satisfying the most of the "required" conditions and the fewest of the
                //"forbidden" conditions. 

                //subtract forbiddens from conditions
                int scoreThisInfo = positivesScore;   // -negativesScore;//from when forbiddens were just a strong suggestion

                if (scoreThisInfo > bestScore)
                {
                    bestScore = scoreThisInfo;
                    best = info;
                }

            }




        }//end findBestAnimInfo
        */

    }// end class RenderAsModelType

        
    public enum ScriptEventCategory
    {
        Exit,
        Enter
    }

    /*
    /// <summary>
    /// Lars: not sure backwards works...
    /// Manual is for letting an outside class (like a goal) set the progress T-value - used by GoalBeingHit (though I'm not sure why regular playback couldn't be used)
    /// </summary>
    public enum Playback
    {
        Forwards,
        Backwards, 
        Manual 
    }
    public enum StartingPoint
    {
        Current,
        FromBeginning,
        Specified
    }
    public enum BlendMode
    {
        Normal,
        NoBlending,
        Additive
    }
    public enum Looping
    {
        Yes,
        No
    }


    */
    

    public class GaitAnimationBracket //: IEdge
    {
 
        /// <summary>
        /// the lower speed (pixels/second) at which this gait is used by a character
        /// </summary>
        public float MinimumSpeed;

        public float MaximumSpeed;

      //  [XmlIgnore]
      //  public float Center;

        public string AnimationKey;

        /// <summary>
        /// the length of one stride. 
        /// If this number is too big, agents' feet will appear to be sliding across the ground.
        /// If it is too small, they will look like treadmilling in place
        /// </summary>
        public float StrideLength;

        /// <summary>
        /// the duration between one leg lifts and hits the ground again
        /// most often 2 strides in an anim, but could also be 4
        /// </summary>
        public float StrideDuration;


        public void Initialize()
        {
        //    Center = MinimumSpeed + (MaximumSpeed - MinimumSpeed) * 0.5f;

        }
    }

}

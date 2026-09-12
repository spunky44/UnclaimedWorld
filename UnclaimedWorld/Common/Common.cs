using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using GameStateManagement;
using System.Threading.Tasks;
using UWGame.SimSide.Maps;
using System.Globalization;
using System.Linq;
using UWGame.SimSide;  
using UWGame.SimSide.AI;
using UWGame.ClientSide;
using UWGame.Control;
using UWGame.Control.Replays;
using UWGame.SimSide.Entities.Biological;
using System.Diagnostics;
using WindowSystem;
namespace UWGame 
{
    public static class Common
    {
        public const double epsilon = 0.0000001;
        public const float floatEpsilon = 0.0001f; //0.00001f; <- did not work in 64 bit!! (Vector3 location comparison) 
        public const decimal decimalEpsilon = 0.00001m;


       /* static float[,] VectorToRadians = new float[3, 3] { { (float)(1.75d * Math.PI), 0, (float)(0.25d * Math.PI) }, 
                                                    { (float)(1.5d * Math.PI), 0, (float)(0.5d * Math.PI) }, 
                                                        { (float)(1.25d * Math.PI), (float)(Math.PI), (float)(0.75d * Math.PI) } };*/
        // dist 0,0 -> 80,80
        // const int mapDistanceSquared = 12800;

        /// <summary>
        /// v = (-1, 0), (0, 1) etc.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
     /*   public static float VectorToRadians45(int x, int y)
        {
            return VectorToRadians[y + 1, x + 1];

        }*/


        /// <summary>
        /// gets an existing entry in the dictionary if it exists, else creates a new entry, adds it to the dictionary and returns it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static U GetExistingEntryOrAddNew<T, U>(Dictionary<T, U> dictionary, T key) where U: new()
        {
            U value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = new U();
                dictionary.Add(key, value);

            }

            return value;
        }

      
        public static void AppendDivider(StringBuilder text)
        {
            AppendLine(text, "---------------");
        }

        public static void AppendDividerOnOwnLine(StringBuilder text)
        {
            AppendLine(text);
            AppendDivider(text);           
        }

        /// <summary>
        /// no word wrap!!!
        /// </summary>
        /// <param name="text"></param>
        /// <param name="t"></param>
        public static void AppendHeader(StringBuilder text, string t)
        {
            text.Append(Label.ToLabel(t, "#COLORHEADER")); // GameData.Instance.GUIConstants.HeaderTintHex));
            text.Append(" \n");
        }

        /// <summary>
        /// no word wrap!!
        /// </summary>
        /// <param name="text"></param>
        /// <param name="t"></param>
        public static void AppendPossibleActionText(StringBuilder text, string t)
        {
            text.Append(Label.ToLabel(t, "#COLORPOSITIVE"));
        }

        /// <summary>
        /// no word wrap!!
        /// </summary>
        /// <param name="text"></param>
        /// <param name="t"></param>
        public static void AppendImpossibleActionText(StringBuilder text, string t)
        {
            text.Append(Label.ToLabel(t, "#COLORNEGATIVE"));
        }

        /*
        public static void AppendActionText(StringBuilder text, string t)
        {
            text.Append(Label.ToLabel(t, "#COLORACTION"));       
        }*/

        public static string ComposeHeadingAndBlobText(string heading, string blob, bool lightBackground = true)
        {
            StringBuilder text = new StringBuilder();
            if (lightBackground)
            {
                AppendHeaderOnLightBG(text, heading);
            }
            else
            {
                AppendHeader(text, heading);
            }

            Append(text, blob);

            return text.ToString();
        }

        public static void AppendHeaderOnLightBG(StringBuilder text, string t)
        {
            text.Append(Label.ToLabel(t, "#COLORHEADERDARK")); // GameData.Instance.GUIConstants.HeaderTintHex));
            text.Append(" \n");
        }
        //

        /// <summary>
        /// for convenience
        /// </summary>
        /// <param name="text"></param>
        /// <param name="t"></param>
        public static void Append(StringBuilder text, string t, bool tintAsValue = false)
        {
            if (tintAsValue)
            {
                text.Append(Label.ToLabel(t, GameData.Instance.GUIConstants.ValueTintHex));         
            }
            else
            {
                text.Append(t);
            }
        }

        public static void Append(StringBuilder text, string t, ValueTint tintValue)
        {
            text.Append(Label.ToLabel(t, GetTint(tintValue)));         
           
        }
        

        public static void AppendPercentage(StringBuilder text, double percentage, bool useColoring, ValueTint? valueTint)
        {
            text.Append(Common.PercentageToString(percentage, useColoring: useColoring, valueTint: valueTint));
        }

        /// <summary>
        /// for convenience
        /// </summary>
        /// <param name="text"></param>
        /// <param name="t"></param>
        public static void AppendFormat(StringBuilder text, string t, bool tintAsValue, params object[] args)
        {
            if (tintAsValue)
            {
                text.Append(Label.ToLabel(string.Format(t, args), GameData.Instance.GUIConstants.ValueTintHex));
            }
            else
            {
                text.AppendFormat(t, args);
            }
        }

        const char indentCharacter = ' ';
        public const string indentString = "   ";
        static int indentLength = indentString.Length;



        /// <summary>
        /// Can also make indented lines, and bullets
        /// StringBuilder.AppendLine uses the default \r\n which TextArea does not recognize...
        /// 
        /// No line breaks!
        /// </summary>
        /// <param name="text"></param>
        /// <param name="line"></param>
       // public static void AppendLine(StringBuilder text, string line, bool indent, string bulletString)
        public static void AppendIndentedLine(StringBuilder text, /*string bulletString,*/ string line)
        {
           /* if (bulletString != null)
            {
                // would only work with monospaced fonts
                string bulletIndent = bulletString;
                bulletIndent = bulletIndent.PadRight(indentLength, indentCharacter);
                text.Append(bulletIndent);
            }
            else
            {*/
                text.Append(indentString);
           // }


            if (line != null)
            {
                text.Append(line);
            }

            text.Append(" \n");
        }

        public static void AppendLine(StringBuilder text, string line = null)
        {
            if (line != null)
            {
                text.Append(line);
            }

            text.Append(" \n");
        }

        /// <summary>
        /// creates the collection if not instantiated, and adds the element
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        public static void AddToList<T>(ref List<T> list, T value)
        {
            if (list == null)
                list = new List<T>();

            list.Add(value);

        }

        public static void AddToList<T>(ref List<T> list, List<T> value)
        {
            if (list == null)
                list = new List<T>();

            list.AddRange(value);

        }

        public static void AddToList<T>(ref HashSet<T> list, T value)
        {
            if (list == null)
                list = new HashSet<T>();

            list.Add(value);

        }

        /// <summary>
        /// helper, that makes null checks on both lists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="listToAdd"></param>
        public static void AddRangeToList<T>(ref List<T> list, List<T> listToAdd)
        {
            if (list == null)
                list = new List<T>();

            if (listToAdd != null)
            {
                list.AddRange(listToAdd);
            }
        }

        /// <summary>
        /// creates the collection if not instantiated, and adds the element
        /// </summary>
        /// <param name="set"></param>
        /// <param name="value"></param>
        public static void AddToSet<T>(ref HashSet<T> set, T value)
        {
            if (set == null)
                set = new HashSet<T>();

            set.Add(value);

        }

        public static void AddRangeToSet<T>(ref HashSet<T> set, List<T> listToAdd)
        {
            if (set == null)
                set = new HashSet<T>();

            foreach (var item in listToAdd)
            {
                set.Add(item);    
            }
            
        }

        public static bool MultiListContains<T, U>(Dictionary<T, List<U>> dictionary, T typeKey, U valueKey)
        {
            List<U> list;
            if (typeKey != null)
            {
                if (dictionary.TryGetValue(typeKey, out list))
                {
                    return list.Contains(valueKey);
                }
            }
            else
            {
                // key is null, search all lists:
                foreach (var item in dictionary)
                {
                    if (item.Value.Contains(valueKey))
                    {
                        return true;
                    }

                }
            }

            return false;
        }

        /// <summary>
        /// creates the collection if not instantiated, and adds the element if it does not exist
        /// </summary>
        /// <param name="list"></param>
        /// <param name="key"></param>
        public static bool AddToDictionary<T,U>(ref Dictionary<T,U> list, T key, U value)
        {
            if (list == null)
                list = new Dictionary<T,U>();

            if (!list.ContainsKey(key))
            {
                list.Add(key, value);

                return true;
            }

            return false;
        }

        public static void AddOrUpdateDictionary<T, U>(ref Dictionary<T, U> list, T key, U value)
        {
            if (list == null)
                list = new Dictionary<T, U>();

            if (!list.ContainsKey(key))
            {
                list.Add(key, value);
            }
            else
            {
                list[key] = value;
            }
        }

        /// <summary>
        /// adds to a dictionary of lists, creates the list and adds it if it does not exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static int AddToMultiList<T, U>(Dictionary<T, List<U>> dictionary, T key, U value)
        {
            List<U> list;
            if (!dictionary.TryGetValue(key, out list))
            {
                list = new List<U>();
                dictionary.Add(key, list);
            }

            list.Add(value);

            return list.Count;
        }

        public static int AddToMultiList<T, U>(ref Dictionary<T, List<U>> dictionary, T key, U value)
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<T, List<U>>();
            }

            List<U> list;
            if (!dictionary.TryGetValue(key, out list))
            {
                list = new List<U>();
                dictionary.Add(key, list);
            }

            list.Add(value);

            return list.Count;
        }

        internal static void AddToDictWithSums<T>(Dictionary<T, float> dictWithSums, T key, float amount)
        {
            float sum;
            if (!dictWithSums.TryGetValue(key, out sum))
            {
                sum = amount;
                dictWithSums.Add(key, sum);
            }
            else
            {
                sum += amount;
                dictWithSums[key] = sum;
            }

        }

        internal static void AddToDictWithSums<T>(Dictionary<T, int> dictWithSums, T key, int amount, out int sum)
        {            
            if (!dictWithSums.TryGetValue(key, out sum))
            {
                sum = amount;
                dictWithSums.Add(key, sum);
            }
            else
            {
                sum += amount;
                dictWithSums[key] = sum;
            }

        }

        /// <summary>
        /// adds to a dictionary that keeps a total instead of each individual item
        /// </summary>
        /// <param name="dictWithSums"></param>
        /// <param name="type"></param>
        internal static void AddToDictWithSums<T>(Dictionary<T, int> dictWithSums, T key, bool addKeyIfNotExisting = true)
        {
            int sum;
            if (!dictWithSums.TryGetValue(key, out sum))
            {
                if (addKeyIfNotExisting)
                {
                    sum = 1;
                    dictWithSums.Add(key, sum);
                }
            }
            else
            {
                sum++;
                dictWithSums[key] = sum;
            }

        }

        internal static void RemoveFromDictWithSums<T>(Dictionary<T, int> dictWithSums, T key)
        {
            int sum;
            if (dictWithSums.TryGetValue(key, out sum))
            {
                sum--;

                if (sum <= 0)
                {
                    dictWithSums.Remove(key);
                }
                else
                {
                    dictWithSums[key] = sum;
                }
            }

        }

        /// <summary>
        /// adds or updates.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="value"></param>
        public static void AddToNestedDictionary<T, U, V>(Dictionary<T, Dictionary<U, V>> dictionary, T key1, U key2, V value)
        {
            Dictionary<U, V> innerDictionary;
            if (!dictionary.TryGetValue(key1, out innerDictionary))
            {
                innerDictionary = new Dictionary<U, V>();
                dictionary.Add(key1, innerDictionary);
            }

            innerDictionary[key2] = value;

        }

        public static void AddToMultiList<T, U>(Dictionary<T, HashSet<U>> dictionary, T key, HashSet<U> value)
        {
            HashSet<U> list;
            if (!dictionary.TryGetValue(key, out list))
            {
                list = new HashSet<U>();
                dictionary.Add(key, list);
            }

            list.UnionWith(value);
        }

        public static void AddToMultiList<T, U>(Dictionary<T, HashSet<U>> dictionary, T key, U value)
        {
            HashSet<U> list;
            if (!dictionary.TryGetValue(key, out list))
            {
                list = new HashSet<U>();
                dictionary.Add(key, list);
            }

            list.Add(value);
        }

        /// <summary>
        /// removes the item with the specified type from the multilist. Only removes the list if empty if that argument is passed.
        /// if the type is not provided (is null), removal will still be performed, but may take longer because all lists have to be searched.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="typeList"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool RemoveFromMultiList<T,U>(Dictionary<T, List<U>> typeList, T key, U value, bool removeEmptyList = false)
        {
            if (typeList != null)
            {
                if (key != null)
                {
                    List<U> list;
                    if (typeList.TryGetValue(key, out list))
                    {
                        bool wasRemoved = list.Remove(value);
                        if (removeEmptyList && list.Count == 0)
                        {
                            typeList.Remove(key);
                        }

                        return wasRemoved;
                    }
                }
                else
                {
                    // we have to search all lists:
                    T keyToRemove = default(T);
                    bool wasRemoved = false;
                    bool removeList = false;

                    foreach (var list in typeList)
                    {
                        if (list.Value.Remove(value))
                        {
                            if (removeEmptyList && list.Value.Count == 0)
                            {
                                keyToRemove = list.Key;
                                removeList = true;
                            }

                            wasRemoved = true;
                            break;
                        }
                    }

                    if (removeList)
                    {
                        typeList.Remove(keyToRemove); 
                    }

                    return wasRemoved;
                }
            }

            return false;
        }


        /// <summary>
        /// removes the item with the specified type from the multilist. Only removes the list if empty if that argument is passed.
        /// if the type is not provided (is null), removal will still be performed, but may take longer because all lists have to be searched.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="typeList"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool RemoveFromMultiSet<T, U>(Dictionary<T, HashSet<U>> typeList, T key, U value, bool removeEmptyList = false)
        {
            if (typeList != null)
            {
                if (key != null)
                {
                    HashSet<U> list;
                    if (typeList.TryGetValue(key, out list))
                    {
                        bool wasRemoved = list.Remove(value);
                        if (removeEmptyList && list.Count == 0)
                        {
                            typeList.Remove(key);
                        }

                        return wasRemoved;
                    }
                }
                else
                {
                    // we have to search all lists:
                    T keyToRemove = default(T);
                    bool wasRemoved = false;
                    bool removeList = false;

                    foreach (var list in typeList)
                    {
                        if (list.Value.Remove(value))
                        {
                            if (removeEmptyList && list.Value.Count == 0)
                            {
                                keyToRemove = list.Key;
                                removeList = true;
                            }

                            wasRemoved = true;
                            break;
                        }
                    }

                    if (removeList)
                    {
                        typeList.Remove(keyToRemove);
                    }

                    return wasRemoved;
                }
            }

            return false;
        }


        public static bool RemoveFromNestedDictionary<T, U, V>(Dictionary<T, Dictionary<U, V>> typeList, T key, U key2, bool removeEmptyList = false)
        {
            if (typeList != null)
            {
                if (key != null)
                {
                    Dictionary<U, V> list;
                    if (typeList.TryGetValue(key, out list))
                    {
                        bool wasRemoved = list.Remove(key2);
                        if (removeEmptyList && list.Count == 0)
                        {
                            typeList.Remove(key);
                        }

                        return wasRemoved;
                    }
                }
              /*  else
                {
                    // we have to search all lists:
                    T keyToRemove = default(T);
                    bool wasRemoved = false;
                    bool removeList = false;

                    foreach (var list in typeList)
                    {
                        if (list.Value.Remove(value))
                        {
                            if (removeEmptyList && list.Value.Count == 0)
                            {
                                keyToRemove = list.Key;
                                removeList = true;
                            }

                            wasRemoved = true;
                            break;
                        }
                    }

                    if (removeList)
                    {
                        typeList.Remove(keyToRemove);
                    }

                    return wasRemoved;
                }*/
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>

        public static float GetMinimumValue(float mean, float deviation)//Returns the 0.1% of the lower part of the standard deviation
        {
            return mean - 3f * deviation;
            
        }
        public static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }

        public static float VectorToAngle(Vector3 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }
        
        public static double GetAngleBetweenVectors(Vector2 a, Vector2 b)
        {
            float dotProduct = Vector2.Dot(a, b); 
            float lengthProduct = a.Length() * b.Length();
            float divOperation = Clamp(dotProduct / lengthProduct, 0f, 1f);

            return Math.Acos(divOperation); // *(180.0 / Math.PI);
        }

        public static Vector2 AngleToVector(float angleInRadians)
        {
            Vector2 angleVector = new Vector2((float)Math.Cos(angleInRadians),
                                              (float)Math.Sin(angleInRadians));

            return angleVector;
        }

      /*  private static Vector3 GetDirectionFromRotation(float rotation)
        {
            return new Vector3((float)Math.Cos(rotation), (float)Math.Sin(rotation), 0f);
        }*/

        /// <summary>
        /// use this method to get the index corresponding to an interval in a float array
        /// </summary>
        /// <param name="number"></param>
        /// <param name="stairSteps"></param>
        /// <returns></returns>
        public static int GetStairStepIndex(float number, float[] stairSteps)
        {
            int maxStairStep = stairSteps.Length - 1;
            int stairstep = maxStairStep;

            int currentStep = 0;

            while (stairstep == maxStairStep && currentStep < maxStairStep)
            {
                if (number < stairSteps[currentStep])
                {
                    stairstep = currentStep;
                }

                currentStep++;
            }

            return stairstep;
        }

        /// <summary>
        /// gets the last edge from the array itself, instead of expecting a parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stairSteps"></param>
        /// <param name="stairstep"></param>
        /// <param name="randomGenerator"></param>
        /// <returns></returns>
        public static T GetStairStepIndexComputeLastEdge<T>(IList<T> stairSteps, out int stairstep, RandomGenerator randomGenerator) where T : IEdge
        {
            float maxEdge = stairSteps[stairSteps.Count - 1].Edge;

            return GetStairStepIndex(stairSteps, out stairstep, randomGenerator, maxEdge);
        }

        /// <summary>
        /// get a random index using the probability edges (increasing numbers) in the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stairSteps"></param>
        /// <param name="stairstep"></param>
        /// <returns></returns>
        public static T GetStairStepIndex<T>(IList<T> stairSteps, out int stairstep, RandomGenerator randomGenerator, float maxEdge = 1f) where T : IEdge
        {
            bool showMessage;
            if (randomGenerator == The.Sim.GameplayRandomGenerator)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }

            float roll = maxEdge * (float)randomGenerator.NextDouble("Common - GetStairStepIndex", showMessage);
            return GetStairStepIndex<T>(roll, stairSteps, out stairstep);
        }

        public static T GetStairStepIndex<T>(float number, IList<T> stairSteps, out int stairstep) where T : IEdge
        {
            int maxStairStep = stairSteps.Count - 1;
            stairstep = maxStairStep;

            int currentStep = 0;

            while (stairstep == maxStairStep && currentStep < maxStairStep)
            {
                if (number <= stairSteps[currentStep].Edge)
                {
                    stairstep = currentStep;
                }

                currentStep++;
            }

            return stairSteps[stairstep];
        }
       
        public static void BuildEdgesFromBucketSizes<T>(List<T> listOfBuckets, bool doSort, out float totalScore) where T: IScore, IEdge
        {
            totalScore = 0;
            foreach (T item in listOfBuckets)
	        {
                totalScore += item.Score;		 
	        }

            if (doSort)
            {
                listOfBuckets.Sort((a, b) => b.Score.CompareTo(a.Score));
            }

            T bucket;
            float lastEdge = 0f;
            for (int i = 0; i < listOfBuckets.Count; i++)
            {
                bucket = listOfBuckets[i];
                               
                bucket.Edge = bucket.Score + lastEdge;

                // changed structs must be re-added:
                listOfBuckets[i] = bucket;

                lastEdge = bucket.Edge;
            }

        }

        /// <summary>
        /// this function will increase a value rapidly at first but easing in when it gets near the target value. Use lerpFactor to control how fast.
        /// This will keep easing forever...
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="lerpFactor"></param>
        /// <returns></returns>
        public static float EaseInValueTowardsTarget(float currentValue, float targetValue, float lerpFactor)
        {
            currentValue = currentValue * (1f - lerpFactor) + targetValue * lerpFactor;

            return currentValue;
        }

        public static Matrix EaseInValueTowardsTarget(Matrix currentValue, Matrix targetValue, float lerpFactor)
        {
            currentValue = currentValue * (1f - lerpFactor) + targetValue * lerpFactor;

            return currentValue;
        }


        //extension methods
        public static Vector2 ToVector2(this Vector3 location)
        {
            return new Vector2(location.X, location.Y);
        }

        public static Vector3 ToVector3(this Vector2 location)
        {
            return new Vector3(location.X, location.Y, 0f);
        }

        public static Point ToPoint(this Vector2 location)
        {
            return new Point((int)location.X, (int)location.Y);
        }

        public static Point ToPoint(this Vector3 location)
        {
            return new Point((int)location.X, (int)location.Y);
        }

        public static Vector2 ToVector2(this Point location)
        {
            return new Vector2(location.X, location.Y);
        }

       /* public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }*/

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                return value.Substring(0, maxLength);
            }

            return value;
        }

        /// <summary>
        /// useful function for getting the closest entity in a list, for instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static T GetMinimum<T>(List<T> list, Func<T, float> function)
        {
            float minimum = float.MaxValue;
            float currentValue;
            T best = default(T);
            foreach (var item in list)
            {
                currentValue = function(item);
                if (currentValue < minimum)
                {
                    minimum = currentValue;
                    best = item;
                }
            }

            return best;
        }

     /*   public static string FractionToIntegerString(double rating)
        {
            //string result = string.Format("{0}: {1}%", StatType, Math.Round(rating, 2) * 100);
            string result = string.Format("{0}", Math.Round(rating, 2) * 100);
            return result;
        }*/

        public static int ToPercent(double rating)
        {
            int percent = (int)(Math.Round(rating, 2) * 100);

            return percent;
        }

        public enum ValueTint { Positive, Negative, Neutral }

        public static string ValueToIntegerString(float value, bool useColoring, ValueTint? valueTint)
        {
            string valueAsString = ((int)value).ToString();
            valueAsString = ApplyColorLabel(true, valueTint, (int)value, valueAsString);

            return valueAsString;
        }

        public static string ValueToDecimalString(float value, bool useColoring, ValueTint? valueTint)
        {
            string valueAsString = DecimalToString(value);
            if (useColoring)
            {
                valueAsString = ApplyColorLabel(true, valueTint, value, valueAsString);
            }

            return valueAsString;
        }

        public static string PercentageToString(double rating, bool includePlusPrefix = false, bool useColoring = false, ValueTint? valueTint = null) // bool? colorNegative = null, bool? colorPositive = null)
        {
            //string result = string.Format("{0}: {1}%", StatType, Math.Round(rating, 2) * 100);
            int percent = ToPercent(rating);

            string prefix = "";
            if (includePlusPrefix && percent >= 0)
            {
                prefix = "+";
            }

            string result = string.Format("{1}{0}%", percent, prefix);

            return ApplyColorLabel(useColoring, valueTint, percent, result);
        }

        private static string ApplyColorLabel(bool useColoring, ValueTint? valueTint, int valueAsNumber, string valueAsString)
        {           
            if (useColoring)
            {
                string tint;
                if (valueTint.HasValue)
                {
                    tint = GetTint(valueTint.Value);
                }
                else
                {
                    tint = GetTint(valueAsNumber);// use the rounded value for display // rating));
                }

                return Label.ToLabel(valueAsString, tint);
            }
            else
            {
                return valueAsString;
            }
        }

        private static string ApplyColorLabel(bool useColoring, ValueTint? valueTint, float valueAsNumber, string valueAsString)
        {
            if (useColoring)
            {
                string tint;
                if (valueTint.HasValue)
                {
                    tint = GetTint(valueTint.Value);
                }
                else
                {
                    tint = GetTint(valueAsNumber);
                }

                return Label.ToLabel(valueAsString, tint);
            }
            else
            {
                return valueAsString;
            }
        }

        public static bool IsPositive(int percent)
        {
            return percent >= 0;
        }

        public static string GetTint(ValueTint valueTint)
        {
            if (valueTint == ValueTint.Positive)
            {
                return GameData.Instance.GUIConstants.PositiveTintHex;
            }
            else if (valueTint == ValueTint.Negative)
            {
                return GameData.Instance.GUIConstants.NegativeTintHex;
            }
            else 
            {
                return GameData.Instance.GUIConstants.ValueTintHex;                
            }
        }

        public static string GetTint(int value)
        {
            if (IsPositive(value))
            {
                return GameData.Instance.GUIConstants.PositiveTintHex;
            }
            else
            {
                return GameData.Instance.GUIConstants.NegativeTintHex;
            }
        }

        public static string GetTint(double value)
        {
            if (IsGreaterThanOrEqual(value, 0d))
            {
                return GameData.Instance.GUIConstants.PositiveTintHex;
            }
            else
            {
                return GameData.Instance.GUIConstants.NegativeTintHex;
            }
        }

        public static string GetTint(float value)
        {
            if (IsGreaterThanOrEqual(value, 0f))
            {
                return GameData.Instance.GUIConstants.PositiveTintHex;
            }
            else
            {
                return GameData.Instance.GUIConstants.NegativeTintHex;
            }
        }

        public static string BoolToString(bool value, bool useColor)
        {
            string tint = null; // Color.Black;
            string text;
            if (value)
            {
                text = "True";
                if (useColor)
                {      
                    tint = GameData.Instance.GUIConstants.PositiveTintHex;
                }
            }
            else
            {
                text = "False";
                if (useColor)
                {
                    tint = GameData.Instance.GUIConstants.NegativeTintHex;
                }
            }


            if (useColor)
            {
                return Label.ToLabel(text, tint);
            }
            else
            {
                return text;
            }
        }

        public static string DecimalToString(double rating)
        {            
            string result = string.Format("{0}", Math.Round(rating, 1));
            return result;
        }

        public static string DecimalToString(float value, int noOfDecimals)
        {
            string result = string.Format("{0}", Math.Round(value, noOfDecimals));
            return result;
        }

        /// <summary>
        /// shows a single digit if low, like 0.0006
        /// or else 34.5
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DecimalToStringSignificant(float value, float? minimum = null, string minimumString = null, float? epsilon = null)
        {
            if (IsZero(value, epsilon))
            {
                return "0";
            }
            else if (minimum.HasValue && value < minimum)
            {
                return "<" + minimumString;
            }
            else if (value < 0.1)
            {
                return value.ToString("G1");
            }
            else
            {
                return Common.DecimalToString(value, 1);
            }

        }

       /* public static string DecimalToStringSignificant(float value, int noOfDigits)
        {
             
            string result = string.Format("{0:G3}", Math.Round(value, noOfDecimals));
            return result;
        }*/

       /* public static string ToLink(string name, long id, bool useUpperCase = false)
        {
            // §E0¤Ward Conlan§
            StringBuilder text = new StringBuilder();
            text.Append("§E");
            text.Append(id.ToString());
            text.Append("¤");

                if (useUpperCase)
                {
                    text.Append(name.ToUpper(Config.Culture));
                }
                else
                {
                    text.Append(name);
                }

        

            text.Append("§");

            return text.ToString();
        }*/

       

        /// <summary>
        /// returns a point on the line between the two locations aat the specified distance, and also checks to see if the target is reachable over the normal terrain map.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="from"></param>
        /// <param name="distanceFromTarget"></param>
        /// <param name="standoffLocation"></param>
        public static void GetLocationAtDistance(Vector3 target, Vector3 from, float distanceFromTarget, out Vector3 standoffLocation)
        {
            if (Common.DistanceOctile(target, from) > distanceFromTarget)
            {
                Vector3 direction = (from - target);
                direction.Normalize();

                Vector3 proposedRendezvous = target + direction * distanceFromTarget;

                if (MapManager.IsPointReachableInStraightLine(target, proposedRendezvous))
                {
                    standoffLocation = proposedRendezvous;
                }
                else
                {
                    // unreachable in a straight line at least. Return the vehicle's location instead.  
                    standoffLocation = target;
                }
            }
            else
            {
                standoffLocation = from; // target;
            }

        }


        /// <summary>
        /// returns the function value of 'number'. The function is a series of datapoints which are linearly interpolated.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="dataPoints"></param>
        /// <returns></returns>
        public static float GetInterpolatedFunctionValue(float number, Vector2[] dataPoints)
        {
            int maxStairStep = dataPoints.Length - 1;
            int stairstep = maxStairStep;

            int currentStep = 0;

            while (stairstep == maxStairStep && currentStep <= maxStairStep) //currentStep < maxStairStep)
            {
                if (number <= dataPoints[currentStep].X) //(number < dataPoints[currentStep].X)
                {
                    stairstep = currentStep; // this breaks out of while.


                    if (currentStep > 0)
                    {
                        Vector2 lastPoint = dataPoints[currentStep - 1];
                        Vector2 thisPoint = dataPoints[currentStep];

                        // interpolate between the two points:
                        return MathHelper.Lerp(lastPoint.Y, thisPoint.Y, (number - lastPoint.X) / (thisPoint.X - lastPoint.X));

                    }
                    else
                    {
                        return dataPoints[0].Y;
                    }
                }

                currentStep++;
            }

            return dataPoints[maxStairStep].Y;

            // return stairstep;
        }

        /// <summary>
        /// moves a value of one normal distribution into another, keeping the 'deviation' in percentage
        /// </summary>
        /// <param name="value"></param>
        /// <param name="oldMean"></param>
        /// <param name="oldStdDeviation"></param>
        /// <param name="newMean"></param>
        /// <param name="newStdDeviation"></param>
        /// <returns></returns>
        public static double MoveValueToNewNormalDistribution(double value, double oldMean, double oldStdDeviation, double newMean, double newStdDeviation)
        {
            //double deviationOfOldValue = Math.Abs((value - oldMean) / (6.0 * oldStdDeviation));
            double percentageDeviationOfOldValue;
            if ((value - oldMean) == 0 || 6.0 * oldStdDeviation == 0)
            {
                percentageDeviationOfOldValue = 0;
            }
            else
            {
                percentageDeviationOfOldValue = Math.Abs((value - oldMean) / (6.0 * oldStdDeviation));
            }

            double newValue = newMean + Math.Sign(value - oldMean) * (percentageDeviationOfOldValue * 6.0 * newStdDeviation);

            return newValue;
        }

        

        /// <summary>
        /// shifts 0 - 1 to -1 to 1
        /// </summary>
        /// <returns></returns>
        public static float ShiftValue(float value)
        {
            return 2f * (value - 0.5f); // shift to -1 - 1

        }


        public static void GetNormalDistributionFromMinMaxValues(float min, float max, out float? mean, out float? stdDev)
        {
            mean = min + (0.5f * (max - min));

            stdDev = (max - mean) / 3f;

        }

        /*public static double RandomSpread(Random random, double averageValue, double spread)
        {
            
            return averageValue + (random.Next(0, 3) - 1) * spread;
        }*/

        public static float RandomBetween(RandomGenerator random, float min, float max)
        {
            bool showMessage;
            if (random.Type == RandomGenerator.GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            return min + (float)random.NextDouble("Common - RandomBetween", showMessage) * (max - min);
        }

        /// <summary>
        /// max is exclusive!
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomBetween(RandomGenerator random, int min, int max)
        {
            bool showMessage;
            if (random.Type == RandomGenerator.GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            return random.Next(min, max, "Common - RandomBetween", showMessage); 
        }

        public static int RandomSign(RandomGenerator random)
        {
            bool showMessage;
            if (random.Type == RandomGenerator.GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            return (2 * random.Next(0, 1, "Common - RandomSign", showMessage) - 1);
        }

        public static Point AddPoints(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static float Average(float f1, float f2)
        {
            return (f1 + f2) * 0.5f;
        }

        /* public static bool PointIsWithin(Point point, Rectangle rect)
         {
             rect.Contains(
         }*/
        /*
        public static float DistanceSquared(Vector3 p1, Vector3 p2)
        {
            return (float)(Math.Pow(p1.X - p2.X, 2f) + Math.Pow(p1.Y - p2.Y, 2f));
        }
        public static float DistanceSquared(Vector3 p1, Vector2 p2)
        {
            return (float)(Math.Pow(p1.X - p2.X, 2f) + Math.Pow(p1.Y - p2.Y, 2f));
        }
        public static int DistanceSquared(Point p1, Point p2)
        {
            return (int)(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        public static int DistanceSquared(AI.Pathfinding.PathFinderNode p1, AI.Pathfinding.PathFinderNode p2)
        {
            return (int)(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        public static int DistanceSquared(int p1x, int p1y, Point p2)
        {
            return (int)(Math.Pow(p1x - p2.X, 2) + Math.Pow(p1y - p2.Y, 2));
        }
        */

        public static float Distance(Point p1, Point p2)
        {
            return (float)(Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)));
        }

        public static float Distance(Vector3 p1, Vector3 p2)
        {
            return Vector3.Distance(p1, p2);
        }

        public static float DistanceOctile(WorldLocation p1, WorldLocation p2)
        {
            return DistanceOctile(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static float DistanceOctile(float p1X, float p1Y, float p2X, float p2Y)
        {
            float dx = Math.Abs(p1X - p2X);
            float dy = Math.Abs(p1Y - p2Y);
            if (dx > dy)
            {
                return dx + dy * 0.5f;
            }
            else
            {
                return dy + dx * 0.5f;
            }
        }

        public static float DistanceOctile(Vector3 p1, WorldLocation p2)
        {
            float dx = Math.Abs(p1.X - p2.X);
            float dy = Math.Abs(p1.Y - p2.Y);
            if (dx > dy)
            {
                return dx + dy * 0.5f;
            }
            else
            {
                return dy + dx * 0.5f;
            }
        }

        public static float DistanceOctile(Vector3 p1, Vector3 p2)
        {
            float dx = Math.Abs(p1.X - p2.X);
            float dy = Math.Abs(p1.Y - p2.Y);
            if (dx > dy)
            {
                float returnvalue = dx + dy * 0.5f;
                return returnvalue;
            }
            else
            {
                return dy + dx * 0.5f;
            }
        }

        public static float DistanceOctile(Vector3 p1, Vector2 p2)
        {
            float dx = Math.Abs(p1.X - p2.X);
            float dy = Math.Abs(p1.Y - p2.Y);
            if (dx > dy)
            {
                return dx + dy * 0.5f;
            }
            else
            {
                return dy + dx * 0.5f;
            }
        }

        public static float DistanceOctile(Vector2 p1, Vector2 p2)
        {
            float dx = Math.Abs(p1.X - p2.X);
            float dy = Math.Abs(p1.Y - p2.Y);
            if (dx > dy)
            {
                return dx + dy * 0.5f;
            }
            else
            {
                return dy + dx * 0.5f;
            }
        }



        /// <summary>
        /// distance between diagonally adjacent "tiles": 1.5f
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float DistanceOctile(Point p1, Point p2)
        {
            int dx = Math.Abs(p1.X - p2.X);
            int dy = Math.Abs(p1.Y - p2.Y);
            if (dx > dy)
            {
                return dx + (float)dy * 0.5f;
            }
            else
            {
                return dy + (float)dx * 0.5f;
            }
        }

        public static float DistanceOctile(TilePos p1, TilePos p2)
        {
            int dx = Math.Abs(p1.X - p2.X);
            int dy = Math.Abs(p1.Y - p2.Y);
            if (dx > dy)
            {
                return dx + (float)dy * 0.5f;
            }
            else
            {
                return dy + (float)dx * 0.5f;
            }
        }

        public static float DistanceOctile(SubtilePos p1, SubtilePos p2)
        {
            int dx = Math.Abs(p1.X - p2.X);
            int dy = Math.Abs(p1.Y - p2.Y);
            if (dx > dy)
            {
                return dx + (float)dy * 0.5f;
            }
            else
            {
                return dy + (float)dx * 0.5f;
            }
        }

        /*
        public static double DistanceEstimate(int distance)
        {
            return Common.ClampBottom(1.0 - ((double)distance / mapDistanceSquared), 0);
        }*/

        public static bool IsZero(double? d1)
        {
            if (d1 == null)
            {
                return false;
            }
            else return IsZero(d1.Value);
        }

        public static bool IsZero(double d1)
        {
            return d1 < epsilon && d1 > -epsilon;
        }

        public static bool IsZero(decimal d1)
        {
            return d1 < decimalEpsilon && d1 > -decimalEpsilon;
        }

        public static bool IsZero(float d1, float? epsilon = null)
        {
            return d1 < (epsilon ?? floatEpsilon) && d1 > -(epsilon ?? floatEpsilon);
        }

        public static bool IsEqual(double d1, double d2)
        {
            return d1 < d2 + epsilon && d1 > d2 - epsilon;
        }

        public static bool IsEqual(double? d1, double? d2)
        {
            if (d2 == null)
            {
                if (d1 == null)
                {
                    return true;
                }
                else return false;
            }
            else
            {
                if (!d1.HasValue)
                {
                    return false;
                }
                else if (Common.IsEqual(d2.Value, d1.Value))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEqual(decimal d1, decimal d2)
        {
            return d1 < d2 + decimalEpsilon && d1 > d2 - decimalEpsilon;
        }

        public static bool IsEqual(float d1, float d2)
        {
            return d1 < d2 + floatEpsilon && d1 > d2 - floatEpsilon;         
        }

        public static bool IsLessThan(float valueToTest, float valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return false;
            }
            else
            {
                return valueToTest < valueToTestWith;
            }
        }

        public static bool IsLessThanOrEqual(float valueToTest, float valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return true;
            }
            else
            {
                return valueToTest < valueToTestWith;  
            }
        }

        public static bool IsLessThanOrEqual(double valueToTest, double valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return true;
            }
            else
            {
                return valueToTest < valueToTestWith;
            }
        }

        public static bool IsGreaterThan(float valueToTest, float valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return false;
            }
            else
            {
                return valueToTest > valueToTestWith;
            }
        }

        public static bool IsGreaterThan(double valueToTest, double valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return false;
            }
            else
            {
                return valueToTest > valueToTestWith;
            }
        }

        public static bool IsGreaterThan(decimal valueToTest, decimal valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return false;
            }
            else
            {
                return valueToTest > valueToTestWith;
            }
        }

        public static bool IsGreaterThanOrEqual(float valueToTest, float valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return true;
            }
            else
            {
                return valueToTest > valueToTestWith;
            }
        }

        public static bool IsGreaterThanOrEqual(double valueToTest, double valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return true;
            }
            else
            {
                return valueToTest > valueToTestWith;
            }
        }

        /*public static bool IsLessThan(float valueToTest, float valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return false;
            }
            else
            {
                return valueToTest < valueToTestWith;
            }
        }*/
        public static bool IsEqual(float d1, float d2, float epsilonToUse)
        {
            return d1 < d2 + epsilonToUse && d1 > d2 - epsilonToUse;
        }

        public static bool IsDirectionEqual(Vector3 v1, Vector3 v2)
        {
            return IsEqual(v1.X, v2.X, directionEpsilon) && IsEqual(v1.Y, v2.Y, directionEpsilon) && IsEqual(v1.Z, v2.Z, directionEpsilon);
        }

        const float locationEpsilon = 0.1f;
        const float directionEpsilon = 0.001f;


        /// <summary>
        /// tuned to the precision used in the game. This used to fail with the samller epsilon values
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool IsLocationEqual(Vector3 v1, Vector3 v2)
        {
            return IsEqual(v1.X, v2.X, locationEpsilon) && IsEqual(v1.Y, v2.Y, locationEpsilon) && IsEqual(v1.Z, v2.Z, locationEpsilon);
        }

        public static bool IsLocationEqual(Vector2 v1, Vector2 v2)
        {
            return IsEqual(v1.X, v2.X, locationEpsilon) && IsEqual(v1.Y, v2.Y, locationEpsilon);
        }

        /*   public static T ClampBottom<T>(T f1, T bottom) where T : IComparable<T> 
           {
               return (f1 > bottom ? f1 : bottom);
           }*/


        public static int Max(int v1, int v2)
        {
            if (v1 >= v2)
                return v1;
            else return v2;
        }

        public static float Max(float v1, float v2)
        {
            if (v1 >= v2)
                return v1;
            else return v2;
        }

        public static int Min(int v1, int v2)
        {
            if (v1 <= v2)
                return v1;
            else return v2;
        }

        public static float Min(float v1, float v2)
        {
            if (v1 <= v2)
                return v1;
            else return v2;
        }

        public static int ClampBottom(int f1, int bottom)
        {
            return (f1 > bottom ? f1 : bottom);
        }
        public static float ClampBottom(float f1, float bottom)
        {
            return (f1 > bottom ? f1 : bottom);
        }
        public static byte ClampBottom(byte f1, byte bottom)
        {
            return (f1 > bottom ? f1 : bottom);
        }
        public static double ClampBottom(double d1, double bottom)
        {
            return (d1 > bottom ? d1 : bottom);
        }
        public static float Clamp(float f1, float bottom, float top)
        {
            return (f1 > bottom ? (f1 < top ? f1 : top) : bottom);
        }

        public static double Clamp(double f1, double bottom, double top)
        {
            return (f1 > bottom ? (f1 < top ? f1 : top) : bottom);
        }

        public static int Clamp(int f1, int bottom, int top)
        {
            return (f1 > bottom ? (f1 < top ? f1 : top) : bottom);
        }

        public static Point ClampPositionToMap(int x, int y)
        {
            
            Point tilePos = new Point(Common.Clamp(x,0,The.Map.mapTileWidth),
                                      Common.Clamp(y, 0, The.Map.mapTileHeight));
            
            return tilePos;
        }

        // use int, then cast...
        /*  public static byte Clamp(byte f1, byte bottom, byte top)
          {
              return (f1 > bottom ? (f1 < top ? f1 : top) : bottom);
          }*/

        public static double ClampTop(double d1, double top)
        {
            return (d1 < top ? d1 : top);
        }

        public static float ClampTop(float d1, float top)
        {
            return (d1 < top ? d1 : top);
        }

        public static int ClampTop(int d1, int top)
        {
            return (d1 < top ? d1 : top);
        }
       /* public static T ClampTop<T>(T d1, T top) where T: 
        {
            return (d1 < top ? d1 : top);
        }*/

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// </summary>
        public static float WrapAngleBetweenMinusPiAndPi(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        //MLo added
        public static Vector3 WrapVectorBetweenMinusNAndN(Vector3 vec, float N )
        {
            return new Vector3(
                WrapFloatBetweenMinusNAndN(vec.X,N),
                WrapFloatBetweenMinusNAndN(vec.Y,N),
                WrapFloatBetweenMinusNAndN(vec.Z,N)
                );

        }
        //MLo added
        public static float WrapFloatBetweenMinusNAndN(float flt, float N )
        {
            while (flt < -N)
            {
                flt += N*2;
            }
            while (flt > N)
            {
                flt -= N*2;
            }
            return flt;
        }



        public static float WrapAngleBetweenZeroAndTwoPi(float radians)
        {
            while (radians < 0)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.TwoPi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        public static bool IntersectionOfTwoLines(Vector2 a, Vector2 b, Vector2 c,
                                           Vector2 d, ref Vector2 result)
        {
            double r, s;

            double denominator = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);

            // If the denominator in above is zero, AB & CD are colinear
            if (denominator == 0)
                return false;

            double numeratorR = (a.Y - c.Y) * (d.X - c.X) - (a.X - c.X) * (d.Y - c.Y);
            //  If the numerator above is also zero, AB & CD are collinear.
            //  If they are collinear, then the segments may be projected to the x- 
            //  or y-axis, and overlap of the projected intervals checked.

            r = numeratorR / denominator;

            //    double numeratorS = (a.Y - c.Y) * (b.X - a.X) - (a.X - c.X) * (b.Y - a.Y);

            //    s = numeratorS / denominator;

            //  If 0<=r<=1 & 0<=s<=1, intersection exists
            //  r<0 or r>1 or s<0 or s>1 line segments do not intersect
            //     if (r < 0 || r > 1 || s < 0 || s > 1)
            //         return false;

            ///*
            //    Note:
            //    If the intersection point of the 2 lines are needed (lines in this
            //    context mean infinite lines) regardless whether the two line
            //    segments intersect, then
            //
            //        If r>1, P is located on extension of AB
            //        If r<0, P is located on extension of BA
            //        If s>1, P is located on extension of CD
            //        If s<0, P is located on extension of DC
            //*/

            // Find intersection point
            result.X = (float)(a.X + (r * (b.X - a.X)));
            result.Y = (float)(a.Y + (r * (b.Y - a.Y)));

            return true;
        }

        //   public enum Direction { North = 0, NorthEast = 1, East = 2, SouthEast = 3, South = 4, SouthWest = 5, West = 6, NorthWest = 7}
        public enum Direction { North = 0, East = 1, South = 2, West = 3, NorthEast = 4, SouthEast = 5, SouthWest = 6, NorthWest = 7 }



        public static Direction GetDirection(Point from, Point to)
        {
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;

            if (dx == 0 && dy == -1)
            {
                return Direction.North; //N
            }
            else if (dx == 1 && dy == -1)
            {
                return Direction.NorthEast; // NE
            }
            else if (dx == 1 && dy == 0)
            {
                return Direction.East; // E
            }
            else if (dx == 1 && dy == 1)
            {
                return Direction.SouthEast; //SE
            }
            else if (dx == 0 && dy == 1)
            {
                return Direction.South; // S
            }
            else if (dx == -1 && dy == 1)
            {
                return Direction.SouthWest; // SW
            }
            else if (dx == -1 && dy == 0)
            {
                return Direction.West; // W
            }
            else if (dx == -1 && dy == -1)
            {
                return Direction.NorthWest; // NW
            }
            return Direction.North;
        }
        public static Direction GetDirection(UWGame.SimSide.AI.Pathfinding.PathFinderNode from, UWGame.SimSide.AI.Pathfinding.PathFinderNode to)
        {
            int dx = to.AbsoluteX - from.AbsoluteX;
            int dy = to.AbsoluteY - from.AbsoluteY;

            if (dx == 0 && dy == -1)
            {
                return Direction.North; //N
            }
            else if (dx == 1 && dy == -1)
            {
                return Direction.NorthEast; // NE
            }
            else if (dx == 1 && dy == 0)
            {
                return Direction.East; // E
            }
            else if (dx == 1 && dy == 1)
            {
                return Direction.SouthEast; //SE
            }
            else if (dx == 0 && dy == 1)
            {
                return Direction.South; // S
            }
            else if (dx == -1 && dy == 1)
            {
                return Direction.SouthWest; // SW
            }
            else if (dx == -1 && dy == 0)
            {
                return Direction.West; // W
            }
            else if (dx == -1 && dy == -1)
            {
                return Direction.NorthWest; // NW
            }
            return Direction.North;
        }
        public static Direction Mirror(Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return Direction.South;
                case Direction.NorthEast:
                    return Direction.SouthWest;
                case Direction.East:
                    return Direction.West;
                case Direction.SouthEast:
                    return Direction.NorthWest;
                case Direction.South:
                    return Direction.North;
                case Direction.SouthWest:
                    return Direction.NorthEast;
                case Direction.West:
                    return Direction.East;
                case Direction.NorthWest:
                    return Direction.SouthEast;
            }
            return Direction.South;
        }


        public static bool IsDiagonal(Direction dir)
        {
            return (dir == Direction.NorthEast ||
                dir == Direction.NorthWest ||
                dir == Direction.SouthEast ||
                dir == Direction.SouthWest);
        }

        /// <summary>
        /// the argument value isn't used, it is only to get the enum type!!!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static T GetRandomEnumValue<T>(T enumType, RandomGenerator randomGenerator)
        {
            Array values = Enum.GetValues(enumType.GetType());
            bool showMessage;
            if (randomGenerator.Type == RandomGenerator.GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            return (T)values.GetValue(randomGenerator.Next(values.Length, "Common - GetRandomEnumValue", showMessage));
        }

        public static T GetRandomListMember<T>(IList<T> list, RandomGenerator randomGenerator)
        {
            bool showMessage;
            if (randomGenerator.Type == RandomGenerator.GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            T randomPerson = list[randomGenerator.Next(list.Count, "Common - GetRandomEnumValue", showMessage)];
            return randomPerson;
        }

        public static int GetRandomListMemberIndex<T>(IList<T> list, RandomGenerator randomGenerator)
        {
            bool showMessage;
            if (randomGenerator.Type == RandomGenerator.GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }
            return randomGenerator.Next(list.Count, "Common - GetRandomEnumValue", showMessage);         
        }

        public static bool IsAdjacent(Point p1, Point p2)
        {
            if (p1 == p2)
            {
                return false;
            }

            if (p1.X == p2.X)
            {
                return Math.Abs(p1.Y - p2.Y) <= 1;
            }
            else if (p1.Y == p2.Y)
            {
                return Math.Abs(p1.X - p2.X) <= 1;
            }
            else
            {
                return (Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y)) == 2;
            }

        }

        public static Dictionary<EntityType, int> GroupItemsByType(List<IKnownEntityData> list)
        {
            // grouping?
            int noOfItems;
            Dictionary<EntityType, int> numberOfItems = new Dictionary<EntityType, int>();

            int numberOfItemsToAdd;
            foreach (var item in list)
            {
                // handle ammo items specially:
                if (item.EntityType.ItemType != null && item.EntityType.ItemType.AmmunitionType != null)
                {
                    // get the ammo total:
                    numberOfItemsToAdd = item.NoOfRounds.Value;
                }
                else
                {
                    numberOfItemsToAdd = 1;
                }

                if (numberOfItems.TryGetValue(item.EntityType, out noOfItems))
                {
                    numberOfItems[item.EntityType] = noOfItems + numberOfItemsToAdd;
                }
                else
                {
                    numberOfItems.Add(item.EntityType, numberOfItemsToAdd);
                }
                //    carryingGrid.AddEntry(item.ItemType.Name, item.ItemType.Name);
            }
            return numberOfItems;
        }

        public static int GetJaggedArrayWidth<T>(T[][] array)
        {
            return array.Length;
        }

        public static int GetJaggedArrayHeight<T>(T[][] array)
        {
            return array[0].Length;
        }

        public static void InitJaggedArray<T>(ref T[][] map, int width, int height)
        {
            map = new T[width][];

            for (int x = 0; x < width; x++)
            {
                map[x] = new T[height];
            }

        }

       /* public static void CopyValues<T>(T[][] from, byte[][] to, int fromXStart, int fromYStart, int width, int height, Func<T, byte> setValue) //, int toXStart = 0, int toYStart = 0)
        {
           // Debug.Assert(Common.GetJaggedArrayWidth(from) <= width && )
            T[] fromColumn;
            byte[] toColumn;

            int toXStart = 0, toYStart = 0;

            // first clamp the from rect:
            Rectangle fromRect = new Rectangle(fromXStart, fromYStart, width, height);
            fromRect = Rectangle.Intersect(fromRect, new Rectangle(0, 0, Common.GetJaggedArrayWidth(from), Common.GetJaggedArrayHeight(from)));


            Rectangle toRect = new Rectangle(toXStart, toYStart, width, height);
            toRect = Rectangle.Intersect(toRect, new Rectangle(0, 0, Common.GetJaggedArrayWidth(to), Common.GetJaggedArrayHeight(to)));

            toRect.X = fromRect.X;
            toRect.Y = fromRect.Y;

            Rectangle intersectRect = Rectangle.Intersect(fromRect, toRect);


           // width = Math.Min(Common.GetJaggedArrayWidth(from), Common.GetJaggedArrayWidth(to));
          //  height = Math.Min(Common.GetJaggedArrayHeight(from), Common.GetJaggedArrayHeight(to));

          //  int fromXEnd = fromXStart + width;
         //   int fromYEnd = fromYStart + height;
            
         
            int toXIndex = toXStart;
            int toYIndex = toYStart;

            for (int fromXIndex = intersectRect.X; fromXIndex < intersectRect.Right; fromXIndex++)
            {
                fromColumn = from[fromXIndex];
                toColumn = to[toXIndex];

                for (int fromYIndex = intersectRect.Y; fromYIndex < intersectRect.Bottom; fromYIndex++)
                {
                    T value = fromColumn[fromYIndex];
                    byte result;
                  
                        result = setValue(value);
                 

                    toColumn[toYIndex] = result;

                    toYIndex++;
                }

                toXIndex++;

            }

        }*/


        public static bool TimepointIsOutDated(double timePoint, double max)
        {
            return The.Sim.TotalUnPausedGameTimeInSeconds - timePoint > max;
        }

        public static void CopyJaggedArray<T>(T[][] fromArray, T[][] toArray)
        {
            // Array.Copy(newAllNodes, AllNodes, newAllNodes.Length);

            System.Diagnostics.Debug.Assert(toArray != null, "error...");

            Parallel.For(0, fromArray.Length, (x) =>
            {
                T[] fromSubArray = fromArray[x];
                T[] toSubArray = toArray[x];
                Array.Copy(fromSubArray, toSubArray, fromSubArray.Length);

            });
        }

        public static T[][] CloneJaggedArray<T>(T[][] fromArray)
        {
            // Array.Copy(newAllNodes, AllNodes, newAllNodes.Length);
            T[][] clone = new T[fromArray.Length][];
            int height = GetJaggedArrayHeight(fromArray);

            Parallel.For(0, fromArray.Length, (x) =>
            {
                clone[x] = new T[height];

            });

            return clone;
        }

        public static void ClearJaggedArray<T>(T[][] map)
        {
            if (map.Length > 15)
            {
                Parallel.For(0, map.Length, (x) =>
                {
                    T[] subArray = map[x];
                    Array.Clear(subArray, 0, subArray.Length);
                });

            }
            else
            {
                foreach (T[] subArray in map)
                {
                    Array.Clear(subArray, 0, subArray.Length);
                }
            }
        }

        public static void ClearMap(byte[][] map)
        {
            byte[] column = map[0];
            int height = column.Length;


            Parallel.For(0, map.Length, (x) =>
            {
                column = map[x];
                for (int y = 0; y < height; y++)
                {
                    column[y] = 0;
                }
            });

            /*  for (int x = 0; x < map.Length; x++)
              {
                  for (int y = 0; y < height; y++)
                  {
                      map[x][y] = 0;
                  }
              }*/

        }

        /*Dictionary<T, TE> dictionary)
            where CATTYPE : ICategoryType
            where T : IHasCategory<CATTYPE>*/

        /*   public static void GroupItemsByCategory(List<Item> list, Dictionary<ItemType, List<Item>> existingList)
           {            
               //int noOfItems;
               List<Item> listOfItems;
              // Dictionary<ItemCategory, List<ItemType>> numberOfItems = new Dictionary<ItemCategory, List<ItemType>>();

               foreach (Item item in list)
               {
                   if (existingList.TryGetValue(item.ItemType, out listOfItems))
                   {
                       if (!listOfItems.Contains(item))
                       {
                           listOfItems.Add(item);
                       }                    
                   }
                   else
                   {
                       listOfItems = new List<Item>();
                       existingList.Add(item.ItemType, listOfItems);
                       listOfItems.Add(item);
                   }

               }

               List<Item> itemsToRemove = null;
               // now remove the items that don't exist in the original list anymore:
               foreach (KeyValuePair<ItemType, List<Item>> kvp in existingList)
               {
                   foreach (Item item in kvp.Value)
                   {
                       if (!list.Contains(item))
                       {
                           if (itemsToRemove == null)
                           {
                               itemsToRemove = new List<Item>();
                           }
                           itemsToRemove.Add(item);
                       }
                   }
               }
               List<Item> listToRemoveFrom;
               List<ItemType> 
               foreach (Item item in itemsToRemove)
               {
                   listToRemoveFrom = existingList[item.ItemType];
                   listToRemoveFrom.Remove(item);
               }
               // clean up the dictionary by removing empty lists:
               existingList.r
            
           }*/

        /// <summary>
        /// shuffle or randomize a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        public static List<T> Randomize<T>(List<T> list, RandomGenerator rnd)
        {
            List<T> randomizedList = new List<T>();
            bool showMessage;
            if (rnd.Type == RandomGenerator.GeneratorType.Sim)
            {
                showMessage = true;
            }
            else
            {
                showMessage = false;
            }

            while (list.Count > 0)
            {
                int index = rnd.Next(0, list.Count, "Common - Randomize", showMessage); //pick a random item from the master list            
                randomizedList.Add(list[index]); //place it at the end of the randomized list            
                list.RemoveAt(index);
            }
            return randomizedList;
        }

        /// <summary>
        /// O(n^2)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="containingList"></param>
        /// <param name="otherList"></param>
        /// <returns></returns>
        public static bool ListContainsRange<T>(List<T> containingList, List<T> otherList)
        {
            foreach (var item in otherList)
            {
                if (!containingList.Contains(item))
                    return false;
            }

            return true;
        }


        public static float FlipOffset(float offset, float width, bool flip)
        {
            if (flip)
            {
                return width - 1 - offset;
            }

            return offset;
        }

        public static Vector3 GetAbsolutePosition(Vector3 location, Vector2 offset, bool flip)
        {
            if (flip)
            {
                return location + new Vector3(-offset.X, offset.Y, 0f);
            }
            else
            {
                return location + new Vector3(offset.X, offset.Y, 0f);
            }

        }


        /// <summary>
        /// Creates an ARGB hex string representation of the <see cref="Color"/> value.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> value to parse.</param>
        /// <param name="includeHash">Determines whether to include the hash mark (#) character in the string.</param>
        /// <returns>A hex string representation of the specified <see cref="Color"/> value.</returns>
        public static string ToHex(this Color color, bool includeHash)
        {
            string[] argb = {
                color.A.ToString("X2"),
                color.R.ToString("X2"),
                color.G.ToString("X2"),
                color.B.ToString("X2"),
            };

            return (includeHash ? "#" : string.Empty) + string.Join(string.Empty, argb);
        }

        /// <summary>
        /// Creates a Color value from an ARGB or RGB hex string.  The string may
        /// begin with or without the hash mark (#) character.
        /// </summary>
        public static Vector3 ToColorVector3(this string hexString)
        {
            return ColorFromHex(hexString).ToVector3();
        }

        public static Color ColorFromHex(this string hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);

            uint hex = uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            Color color = Color.White;

            if (hexString.Length == 8)
            {
                color.A = (byte)(hex >> 24);
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            else if (hexString.Length == 6)
            {
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            else
            {
                throw new InvalidOperationException("Invalid hex representation of an ARGB or RGB color value.");
            }

            return color;
        }

        public static float DecreaseValueBetweenZeroAndOne(float value, float decreaseAmountPerDay, double deltaTimeInSeconds)
        {
            if (value > 0f)
            {
                value = (float)Common.ClampBottom(value - decreaseAmountPerDay * deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond, 0f);
            }

            return value;
        }

        public static float DecreaseValueBetweenZeroAndOneBySecondsAmount(float value, float decreaseAmountPerSecond, double deltaTimeInSeconds)
        {
            if (value > 0f)
            {
                value = (float)Common.ClampBottom(value - decreaseAmountPerSecond * deltaTimeInSeconds, 0f);
            }

            return value;
        }

        public static float IncreaseValueBetweenZeroAndOne(float value, float increaseAmountPerDay, double deltaTimeInSeconds)
        {
            if (value < 1f)
            {
                value = (float)Common.ClampTop(value + increaseAmountPerDay * deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond, 1f);
            }

            return value;
        }

        public static void IncreaseValueBetweenZeroAndOne(ref double value, float increaseAmountPerDay, double deltaTimeInSeconds)
        {
            if (value < 1.0)
            {
                value = Common.ClampTop(value + increaseAmountPerDay * deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond, 1.0);
            }

        }

        public static float IncreaseValueBetweenZeroAndTopLimit(float value, float increaseAmountPerDay, double deltaTimeInSeconds, float topLimit)
        {
            if (value < topLimit)
            {
                value = (float)Common.ClampTop(value + increaseAmountPerDay * deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond, topLimit);
            }

            return value;
        }



        public static float RoughVectorMagnitude( Vector3 vec)
        {
           float sTempV;

           float sMaxV = Math.Abs(vec.X);
           float sMedV = Math.Abs(vec.Y);
           float sMinV = Math.Abs(vec.Z);

           if (sMaxV < sMedV)
           {
              sTempV = sMaxV;
              sMaxV = sMedV;
              sMedV = sTempV;
           }
   
           if (sMaxV < sMinV)
           {
              sTempV = sMaxV;
              sMaxV = sMinV;
              sMinV = sTempV;
           }
   
           sMedV += sMinV;
           sMaxV += (sMedV*0.25f); 
   
           return sMaxV;

        }

        public static string GetPriceAsString(decimal? price, bool useColoring = false, ValueTint? tintToUse = null)
        {
            if (price.HasValue)
            {
                string text = Common.MoneyAsString(price.Value, false);

                text = ApplyColorLabel(useColoring, tintToUse, (float)price.Value, text);

                return text;
            }
            else return "";
        }


        const string thousandsPostfix = "k";

        /// <summary>
        /// 474.2
        /// 8.23k
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="abbreviate"></param>
        /// <returns></returns>
        public static string MoneyAsString(decimal amount, bool abbreviate = false)
        {
            if (abbreviate) // abb. to max 5 characters...  that is 3 significant digits
            {
                if (amount >= 1000m)
                {
                    decimal thousands = amount / 1000m;

                    return thousands.ToString("G3") + thousandsPostfix;
                }
                else 
                {
                    // one decimal:
                    return amount.ToString("N1"); 

                    // four significant digits:
                    //return amount.ToString("G4");
                }
                /*
                else if (amount >= 100m)
                {
                    // 474.2
                    // use one decimal:
                    return amount.ToString("N1");
                }
                else
                {
                    // 47.25
                    // use 2 decimals:
                    return amount.ToString("N2");
                }*/
            }

            return amount.ToString("N1");  // amount.ToString("N2");            

        }

        public static string ListToCommaSeparatedString<T>(IList<T> items, Func<T, string> getName) //, bool useAndForLast = true)
        {
            if (items == null || items.Count == 0)
                return "";

            string names = "";
            string delim = "";
            for (int i = items.Count - 1; i >= 0; i--) // why backwards..?
            {
                T item = items[i];

                names = getName(item) + delim + names;

                if (delim == "")
                {
                    delim = " and ";
                }
                else if (delim == " and ")
                {
                    delim = ", ";
                }
            }

            return names;
        }

        public static Color GetRandomColorFromSeed(uint color)
        {
            return new Color((byte)((color * 100) % 255), (byte)((color * 23) % 255), (byte)((color * 7) % 255));
        }

        /// <summary>
        /// uses LINQ. Be careful to avoid deferred execution - best to convert the result to List right away.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequences"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            // base case: 
            IEnumerable<IEnumerable<T>> result = new[] { Enumerable.Empty<T>() };
            foreach (var sequence in sequences)
            {
                var s = sequence; // don't close over the loop variable 
                // recursive case: use SelectMany to build the new product out of the old one 
                result =
                  from seq in result
                  from item in s
                  select seq.Concat(new[] { item });
            }
            return result;
        }

        #region influence maps

  


        public static float CalculateProgressDelta(double seconds, float gameDaysNeeded)
        {
            return (float)((seconds * The.Sim.DateAndTime.DaysPerSecond) / gameDaysNeeded);               

        }


       


       
      

        #endregion


       
    }

    public interface IScore
    {
        float Score { get; set; }
    }


}

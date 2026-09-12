using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Entities
{
    public class SimEffectsComponent: Component
    {
        /// <summary>
        /// for side panel listing
        /// </summary>
        public List<SimEffectProfile> EffectProfiles;

        /// <summary>
        /// for quick attribute lookup
        /// </summary>
        public Dictionary<AffectsNumbers, List<SimEffect>> NumberEffects;
        Dictionary<AffectsNumbers, List<SimEffectID>> snapshotNumberEffects;

        public Dictionary<AffectsFlags, List<SimEffect>> FlagEffects;
        Dictionary<AffectsFlags, List<SimEffectID>> snapshotFlagEffects; 


        public SimEffectsComponent()
        {
           
        }

        public SimEffectsComponent(Entity parent)
            : base(parent, 1d / GameData.Instance.Constants.UpdateIntervalForEntityComponents)
        {

            NumberEffects = new Dictionary<AffectsNumbers, List<SimEffect>>();
            FlagEffects = new Dictionary<AffectsFlags, List<SimEffect>>();

            EffectProfiles = new List<SimEffectProfile>();
        }


        /// <summary>
        /// will replace any existing profiles of the same type before starting the new effect
        /// </summary>
        /// <param name="effect"></param>
        public void Start(EffectProfileType effect)
        {
            Remove(effect);
            

            SimEffectProfile profile = new SimEffectProfile(effect, Parent);
            EffectProfiles.Add(profile);

            foreach (var item in profile.Effects)
            {
                NumberEffectType numberEffect = item.EffectType as NumberEffectType;
                if (numberEffect != null)
                {
                    Common.AddToMultiList(NumberEffects, numberEffect.Affects, item);
                }

                FlagEffectType flagEffect = item.EffectType as FlagEffectType;
                if (flagEffect != null)
                {
                    Common.AddToMultiList(FlagEffects, flagEffect.Affects, item);
                }
            }

            

            Parent.RecomputeUpdateInterval();

        }

        /// <summary>
        /// removes any existing profiles of the same type.
        /// </summary>
        /// <param name="effect"></param>
        public void Remove(EffectProfileType effect)
        {
            SimEffectProfile existingProfile = EffectProfiles.FirstOrDefault(e => e.EffectProfileType == effect);
            if (existingProfile != null)
            {
                DestroyProfile(existingProfile, true);
            }
        }

        private void DestroyProfile(SimEffectProfile existingProfile, bool wasReplaced)
        {
            existingProfile.Destroy(Parent, wasReplaced);
            EffectProfiles.Remove(existingProfile);
            foreach (var item in existingProfile.Effects)
            {
                RemoveEffect(item);
            }

        }

        private void RemoveEffect(SimEffect item)
        {
            NumberEffectType numberEffect = item.EffectType as NumberEffectType;
            if (numberEffect != null)
            {
                Common.RemoveFromMultiList(NumberEffects, numberEffect.Affects, item);
            }

            FlagEffectType flagEffect = item.EffectType as FlagEffectType;
            if (flagEffect != null)
            {
                Common.RemoveFromMultiList(FlagEffects, flagEffect.Affects, item);
            }
        }

        public bool GetEffect(AffectsFlags affects, bool baseValue, string typeKey = null, string typeTag = null, List<Tuple<string, bool>> effectComponents = null)
        {
            // we can easily cache constant values 
            // how do we cache values with falloff... cache them each frame?

            List<SimEffect> effects;
            bool value = baseValue;
            if (FlagEffects.TryGetValue(affects, out effects))
            {               
                IEnumerable<SimEffect> effectsToIterate = GetEffectsToIterate(typeKey, typeTag, effects);

                if (effectsToIterate != null)
                {
                    foreach (var item in effectsToIterate)
                    {
                        FlagEffectType flagEffect = item.EffectType as FlagEffectType;

                        value = value && flagEffect.Value; // ??

                        GatherComponent(effectComponents, item, flagEffect);

                        /*if (flagEffect.Operator == NumberEffectOperator.Multiply)
                        {
                            value *= item.GetValue();
                        }*/
                    }
                    /*
                    foreach (var item in effectsToIterate)
                    {
                        NumberEffect numberEffect = item.EffectType as NumberEffect;
                        if (numberEffect.Operator == NumberEffectOperator.Add)
                        {
                            value += item.GetValue();
                        }
                    }*/
                }
            }

            return value;
        }

        private static void GatherComponent(List<Tuple<string, bool>> effectComponents, SimEffect item, FlagEffectType flagEffect)
        {
            if (effectComponents != null)
            {
                effectComponents.Add(new Tuple<string, bool>(item.EffectType.Name, flagEffect.Value));
            }
        }

        private static void GatherComponent(List<Tuple<string, NumberEffectOperator, float>> effectComponents, SimEffect item, NumberEffectType effect, float componentValue)
        {
            if (effectComponents != null)
            {
                effectComponents.Add(new Tuple<string, NumberEffectOperator, float>(item.EffectType.Name, effect.Operator, componentValue));
            }
        } 

        public float GetEffect(AffectsNumbers affects, float baseValue, string typeKey = null, string typeTag = null, List<Tuple<string, NumberEffectOperator, float>> effectComponents = null)
        {
            // we can easily cache constant values 
            // how do we cache values with falloff... cache them each frame?

            List<SimEffect> effects;
            float value = baseValue;
            if (NumberEffects.TryGetValue(affects, out effects))
            {
                IEnumerable<SimEffect> effectsToIterate = GetEffectsToIterate(typeKey, typeTag, effects);

                if (effectsToIterate != null)
                {
                    foreach (var item in effectsToIterate)
                    {
                        NumberEffectType numberEffect = item.EffectType as NumberEffectType;
                        if (numberEffect.Operator == NumberEffectOperator.Multiply)
                        {
                            float componentValue = item.GetValue();
                            value *= componentValue;

                            GatherComponent(effectComponents, item, numberEffect, componentValue);
                        }

                       
                    }

                    foreach (var item in effectsToIterate)
                    {
                        NumberEffectType numberEffect = item.EffectType as NumberEffectType;
                        if (numberEffect.Operator == NumberEffectOperator.Add)
                        {
                            float componentValue = item.GetValue();
                            value += componentValue;

                            GatherComponent(effectComponents, item, numberEffect, componentValue);                   
                        }
                    }
                }
            }

            return value;
        }

        private static IEnumerable<SimEffect> GetEffectsToIterate(string typeKey, string typeTag, List<SimEffect> effects)
        {
            IEnumerable<SimEffect> effectsToIterate = null;
            if (typeKey != null || typeTag != null)
            {
                HashSet<SimEffect> effectsSet = null;
                if (typeKey != null)
                {
                    // filter.. cache these in a dict?
                    foreach (var item in effects)
                    {
                        if (item.EffectType.AffectsTypeKey != null && item.EffectType.AffectsTypeKey.Contains(typeKey))
                        {
                            Common.AddToList(ref effectsSet, item);
                        }
                    }
                }

                if (typeTag != null)
                {
                    foreach (var item in effects)
                    {
                        if (item.EffectType.AffectsTypeTag != null && item.EffectType.AffectsTypeTag.Contains(typeTag))
                        {
                            Common.AddToList(ref effectsSet, item);
                        }
                    }
                }

                effectsToIterate = effectsSet;
            }
            else
            {
                effectsToIterate = effects;
            }
            return effectsToIterate;
        }


        public override double? GetUpdateInterval()
        {
            double? tempInterval = null, currentInterval = null;

            // return the expiry timepoint for any effect:
            foreach (var item in NumberEffects)
            {
                foreach (var effect in item.Value)
                {
                    if (effect.ExpiresOn.HasValue)
                    {
                        tempInterval = UpdateTimePoints.ComputeIntervalFromTimepoint(effect.ExpiresOn);
                        UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);         
                    }
                }               

            }

            return currentInterval;            
        }


      //  protected override void UpdateRegulated(double? timeSinceLastUpdate)
        protected override void UpdatePlaySiteRegulated(double? timeSinceLastUpdate)
        {
            base.UpdatePlaySiteRegulated(timeSinceLastUpdate);

            List<SimEffect> destroyedEffects = null;
            foreach (var item in EffectProfiles)
            {
                foreach (var effect in item.Effects)
                {
                    bool wasDestroyed;
                    effect.UpdateExpiry(out wasDestroyed);

                    if (wasDestroyed)
                    {
                        Common.AddToList(ref destroyedEffects, effect);
                    }
                }
            }

            /*
            foreach (var item in Effects)
            {
                foreach (var effect in item.Value)
                {
                    bool wasDestroyed;
                    effect.UpdateExpiry(out wasDestroyed);
                       
                    if (wasDestroyed)
                    {
                        Common.AddToList(ref destroyedEffects, effect);
                    }                    
                }
            }*/

            if (destroyedEffects != null)
            {
                foreach (var effect in destroyedEffects)
                {
                    RemoveEffect(effect);
                   
                    foreach (var item in EffectProfiles)
                    {
                        item.Effects.Remove(effect);
                    }
                }

                for (int i = EffectProfiles.Count - 1; i >= 0; i--)
                {
                    SimEffectProfile profile = EffectProfiles[i];
                    if (profile.Effects.Count == 0)
                    {
                        DestroyProfile(profile, false);
                        //EffectProfiles.RemoveAt(i);
                    }
                }
            }
        }


        #region ISnapshot

       
        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

          //  Effects = sn.DoMultiMap(Effects);
            EffectProfiles = sn.DoList(EffectProfiles);

            if (NumberEffects != null)
            {
                snapshotNumberEffects = new Dictionary<AffectsNumbers, List<SimEffectID>>();
                foreach (var item in NumberEffects)
                {
                    snapshotNumberEffects[item.Key] = item.Value.Select(s => s.ID).ToList();
                }

            }

            if (FlagEffects != null)
            {
                snapshotFlagEffects = new Dictionary<AffectsFlags, List<SimEffectID>>();
                foreach (var item in FlagEffects)
                {
                    snapshotFlagEffects[item.Key] = item.Value.Select(s => s.ID).ToList();
                }

            }

            snapshotNumberEffects = sn.DoMultiMap(snapshotNumberEffects);
            snapshotFlagEffects = sn.DoMultiMap(snapshotFlagEffects);

            sn.Ignore(NumberEffects);
            sn.Ignore(FlagEffects);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (snapshotNumberEffects != null)
            {
                NumberEffects = new Dictionary<AffectsNumbers, List<SimEffect>>();

                foreach (var item in snapshotNumberEffects)
                {
                    NumberEffects.Add(item.Key, item.Value.Select(s => LookUp<SimEffect, SimEffectID>.FindByID(s)).ToList());
                }
              
                snapshotNumberEffects.Clear(); // remember to clear/set to null for the next save
            }

            if (snapshotFlagEffects != null)
            {
                FlagEffects = new Dictionary<AffectsFlags, List<SimEffect>>();

                foreach (var item in snapshotFlagEffects)
                {
                    FlagEffects.Add(item.Key, item.Value.Select(s => LookUp<SimEffect, SimEffectID>.FindByID(s)).ToList());
                }

                snapshotFlagEffects.Clear(); // remember to clear/set to null for the next save
            }

            if (EffectProfiles != null)
            {
                foreach (var item in EffectProfiles)
                {
                    item.LoadPostProcess(sn);
                }
            }

        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.SimEffects
{
    public enum SimEffectID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// a symptom can have more effects
    /// 
    /// effects that change a property over time should probably be made processes?
    /// </summary>
    public class SimEffect: ISnapshot, ILookUp<SimEffect, SimEffectID>
    {
        public EffectType EffectType;

        /// <summary>
        /// in seconds - useful for effects that have falloff
        /// </summary>
        public double StartedOn;

        
        /// <summary>
        /// food items will scale the effect type intensity by consumed bulk
        /// </summary>
        public float? Intensity;
        

        public double? ExpiresOn;

        public SimEffect()
        {
        }

        public SimEffect(EffectType type)
        {
            AddToLookup();

            this.EffectType = type;
                        
            Intensity = EffectType.GetIntensity(); //.Intensity;

            StartedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
         //   Intensity = intensity;

            if (EffectType.DurationInDays.HasValue)
            {
                ExpiresOn = StartedOn + (DateAndTime.secondsPerDay * EffectType.DurationInDays.Value);

            }
            else if (EffectType.DynamicDurationInDays != null)
            {
                PropertyResult? result =  EffectType.DynamicDurationInDays.Evaluate(null, null, null, null);
                if (result.HasValue)
                {
                    ExpiresOn = StartedOn + (DateAndTime.secondsPerDay * result.Value.NumberResult);
                }
            }

           
        }


       /* public void Start(EffectType effectType, float intensity)
        {
            this.EffectType = effectType;

           
        }*/

        public void Destroy()
        { 
            RemoveIDEntry();
        }

        /// <summary>
        /// the object expires x seconds after the last agent updarte was received.
        /// </summary>
        /// <param name="wasDestroyed"></param>
        public void UpdateExpiry(out bool wasDestroyed)
        {
            wasDestroyed = false;

            if (ExpiresOn.HasValue)
            {
                if (The.Sim.TimepointReached(ExpiresOn.Value))
                {
                    Destroy(); // destroy directly
                    wasDestroyed = true;
                }
            }
        }



        public float GetValue()
        {
            return Intensity.Value;

            /* TODO
            switch(EffectType.Falloff)
            {
                case Falloff.None:
                    return Intensity;

                case Falloff.Linear:
                    return;

            }*/


        }

        #region ILookup

        private SimEffectID id = SimEffectID.Invalid;
        static SimEffectID IDCounter = SimEffectID.First;

        public SimEffectID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public SimEffectID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= SimEffectID.Max)
            {
                throw new Exception("Astounding, SimEffectID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public SimEffectID SnapshotID(Snapshotter sn, SimEffectID id)
        {
            return (SimEffectID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != SimEffectID.Invalid)
                LookUp<SimEffect, SimEffectID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = SimEffectID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<SimEffect, SimEffectID>.Remove(this);
        }

        void ILookUp<SimEffect, SimEffectID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = SimEffectID.First;
        }

        void ILookUp<SimEffect, SimEffectID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<SimEffect, SimEffectID>.Create();
        }


        #endregion

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            #region IDs
            id = this.SnapshotID(sn, id);
            IDCounter = sn.DoEnum(IDCounter);
            #endregion

            EffectType = sn.DoGameData(EffectType);
            StartedOn = sn.DoDouble(StartedOn);
            Intensity = sn.DoFloatNullable(Intensity);
            ExpiresOn = sn.DoDoubleNullable(ExpiresOn);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

          
        }


        #endregion

    }
}

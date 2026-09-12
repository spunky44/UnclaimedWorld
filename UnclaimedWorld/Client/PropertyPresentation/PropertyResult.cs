using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide;

namespace UWGame.ClientSide.PropertyPresentation
{
    /// <summary>
    /// move to Sim!!
    /// </summary>
    public struct PropertyResult: ISnapshot // gets snapshotted in BodyPart, Site...
    {
        public string PropertyKeyName;

        //Only one of these values will be filled out.

        /// <summary>
        /// another object upon which further getProperty calls be made
        /// This was to be used by GetObject, but there has not been any uses of it, so i am commenting it out...
        /// </summary>
       // public IHasExposedProperties HasExposedPropertiesResult;

        public string StringResult;

        public float? NumberResult;

        /// <summary>
        /// used for the 2 numbers for the happiness display
        /// </summary>
        public Pair<float, float> NumberPairResult;

        public bool? BoolResult;

        public Vector2? LocationResult;

        public DateAndTime.TimeDateYear? DateResult;

       // public RatingResult RatingResult;  


        /// <summary>
        /// don't think object arrays can be snapshotted... so making this a list instead. for now, only used in icon display
        /// </summary>
        public List<string> MultiResults;
       // public object[] MultiResults;

        public override string ToString()
        {
            if (StringResult != null)
            {
                return StringResult;
            }
            else if (BoolResult.HasValue)
            {
                return BoolResult.ToString();
            }
            else if (NumberResult.HasValue)
            {
                return NumberResult.Value.ToString();
            }
            else if (LocationResult.HasValue)
            {
                return LocationResult.Value.ToString();
            }
            else if (NumberPairResult != null)
            {
                return NumberPairResult.ToString();
            }
            else if (MultiResults != null)
            {
                return string.Concat(MultiResults.ToArray());
            }
            else if (DateResult != null)
            {
                return DateResult.Value.ToString();
            }

            return "";
        }


        /// <summary>
        /// uses Round to try to avoid floating point imprecision issues...
        /// </summary>
        /// <returns></returns>
        public int? GetIntegerResult()
        {
            if (NumberResult.HasValue)
            {
                return (int)Math.Round(NumberResult.Value);
            }
            else return null;
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.PropertyKeyName = sn.DoString(PropertyKeyName);
            this.BoolResult = sn.DoBoolNullable(BoolResult);
            this.LocationResult = sn.DoVector2Nullable(LocationResult);
            this.NumberResult = sn.DoFloatNullable(NumberResult);
            this.NumberPairResult = sn.DoPair(NumberPairResult); 
            this.StringResult = sn.DoString(StringResult);
            this.MultiResults = sn.DoList(MultiResults);
            this.DateResult = sn.DoTimeDateYearNullable(DateResult);

            //HasExposedPropertiesResult // ???

            return this;
        }

       
        Snapshotter.Version version;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}

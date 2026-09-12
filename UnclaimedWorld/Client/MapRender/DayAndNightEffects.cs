using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide;

namespace UWGame.ClientSide.Map
{
    public class DayAndNightEffects
    {
        public Curve TimeOfDayAlpha;
        public Curve TimeOfDayReds;
        public Curve TimeOfDayGreens;
        public Curve TimeOfDayBlues;

        public enum SunAnimations { Night, MorningBeforeSunrise, MorningAfterSunrise, Day, EveningBeforeSunset, EveningAfterSunset }
        
        public SunAnimations SunAnimation;


        private const float sunriseOnCurve = 42f;
        private const float sunriseOnCurveEnds = 80f;
        private const float sunsetOnCurveStarts = 191f;
        private const float sunsetOnCurve = 242; //235;

        private const float sundiskHitsHorizon = 0.040f;


        Matrix shadowWarping = Matrix.CreateScale(new Vector3(1.2f, 0.8f, 1f)); // Matrix.CreateScale(new Vector3(1.15f, 0.85f, 1f)); // Matrix.CreateScale(new Vector3(1.4f, 0.8f, 1f));

        public Matrix SunShadowRotationMatrix;
        public Matrix ShadowScaling;
        public float ShadowLength;

        /// <summary>
        /// the shadow direction DOT the x-direction (how 'horizontal')
        /// </summary>
        public float ShadowXAlignment;

        private const float maxShadowLengthScaling = 10f;
         
        private Matrix? shadowMatrix;
        private Plane lightPlane = new Plane(-Vector3.UnitZ, 0);
       

        public void LoadContent()
        {
            TimeOfDayAlpha = The.Client.Content.Load<Curve>("TimeOfDay_Alpha");
            TimeOfDayReds = The.Client.Content.Load<Curve>("TimeOfDay_Reds");
            TimeOfDayGreens = The.Client.Content.Load<Curve>("TimeOfDay_Greens");
            TimeOfDayBlues = The.Client.Content.Load<Curve>("TimeOfDay_Blue");
        }


        public void Recompute()
        {
            shadowMatrix = null;

            SunAnimation = GetAnimation();

            if (The.Sim.DateAndTime.SunIsUp)
            {
                // not sure if it is important to use the unshifted Azimuth...
                ShadowXAlignment = MathHelper.SmoothStep(0f, 1f, Math.Abs(The.Sim.DateAndTime.UnshiftedAzimuth) / MathHelper.PiOver2);


                SunShadowRotationMatrix = Matrix.CreateRotationZ(The.Sim.DateAndTime.SunAzimuth - MathHelper.Pi) * shadowWarping; // warp the shadows so they are aligned with the model shadows

                ShadowLength = MathHelper.Clamp((float)(1.0 / Math.Tan(The.Sim.DateAndTime.SunElevation)), 0.1f, maxShadowLengthScaling);

                ShadowLength = MathHelper.SmoothStep(0.1f, maxShadowLengthScaling, ShadowLength / maxShadowLengthScaling);

                // make long shadows slimmer:
                ShadowScaling = Matrix.CreateScale(1f - 0.5f * (ShadowLength / maxShadowLengthScaling), ShadowLength, 1f);
            }
        }

        public Color? GetTimeOfDayColor()
        {
            float time = 120f, progress;
            switch (SunAnimation)
            {
                case SunAnimations.MorningBeforeSunrise:
                    progress = The.Sim.DateAndTime.GetProgress(DateAndTime.dawnSunElevation);
                    time = progress * sunriseOnCurve;
                    break;
                case SunAnimations.MorningAfterSunrise:
                    progress = The.Sim.DateAndTime.SunElevation / DateAndTime.dawnSunElevationEnd;
                    // map to timescale on curve:
                    time = MathHelper.Lerp(sunriseOnCurve, sunriseOnCurveEnds, progress); // 0 to 80
                    break;
                case SunAnimations.Day:
                    return null;
                case SunAnimations.EveningBeforeSunset:
                    progress = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, DateAndTime.sunsetElevationStart) / DateAndTime.sunsetElevationStart;
                    time = MathHelper.Lerp(sunsetOnCurveStarts, sunsetOnCurve, progress);
                    break;
                case SunAnimations.EveningAfterSunset:
                    progress = Math.Abs(The.Sim.DateAndTime.SunElevation / DateAndTime.sunsetElevationEnd); //DateAndTime.Instance.GetEveningAnimationProgress();
                    time = MathHelper.Lerp(sunsetOnCurve, 255f, progress);
                    break;
                case SunAnimations.Night:
                    time = 255f;
                    break;
            }

            Color tint = new Color(
                   (byte)TimeOfDayReds.Evaluate(time),
                   (byte)TimeOfDayGreens.Evaluate(time),
                   (byte)TimeOfDayBlues.Evaluate(time),
                   (byte)TimeOfDayAlpha.Evaluate(time));

            return tint;

        }

        public SunAnimations GetAnimation()
        {
            if (The.Sim.DateAndTime.TimeOfDay < 0.5) // after midnight, before noon A.M
            {
                if (The.Sim.DateAndTime.SunElevation > DateAndTime.dawnSunElevation)
                {
                    if (The.Sim.DateAndTime.SunElevation < 0f)
                    {
                        return SunAnimations.MorningBeforeSunrise;
                    }
                    else if (The.Sim.DateAndTime.SunElevation < DateAndTime.dawnSunElevationEnd)
                    {
                        return SunAnimations.MorningAfterSunrise;
                    }
                    else
                    {
                        return SunAnimations.Day;
                    }
                }
                else return SunAnimations.Night;
            }
            else // P.M
            {
                if (The.Sim.DateAndTime.SunElevation < DateAndTime.sunsetElevationStart)
                {
                    if (The.Sim.DateAndTime.SunElevation > 0f)
                    {
                        return SunAnimations.EveningBeforeSunset;
                    }
                    else if (The.Sim.DateAndTime.SunElevation > DateAndTime.sunsetElevationEnd)
                    {
                        return SunAnimations.EveningAfterSunset;
                    }
                    else
                    {
                        return SunAnimations.Night;
                    }
                }
                else return SunAnimations.Day;
            }
        }

        public Vector3? GetDropShadowEstimate()
        {
            switch (SunAnimation)
            {
                case SunAnimations.Day:
                case SunAnimations.EveningBeforeSunset:
                case SunAnimations.MorningAfterSunrise:
                    Vector3 shadowVector = Vector3.Transform(-Vector3.UnitZ * ShadowLength, GroundObjectsShadowMatrix);
                    return shadowVector;
            }

            return null;
        }

      
        public Matrix GroundObjectsShadowMatrix
        {
            get
            {
                if (shadowMatrix != null)
                {
                    return shadowMatrix.Value;
                }
                else
                {
                    shadowMatrix = Matrix.CreateShadow(The.Sim.DateAndTime.SunPosition, lightPlane);
                    return shadowMatrix.Value;
                }
            }
        }

        public float GetDropShadowAlphaFactor()
        {
            float progress;
            switch (SunAnimation)
            {
                case SunAnimations.MorningBeforeSunrise:
                    return 0f;
                case SunAnimations.MorningAfterSunrise:
                    if (The.Sim.DateAndTime.SunElevation > sundiskHitsHorizon)
                    {
                        return 1f;
                    }
                    else
                    {
                        progress = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, sundiskHitsHorizon) / sundiskHitsHorizon;
                        return MathHelper.Lerp(1f, 0f, progress);
                    }
                case SunAnimations.Day:
                    return 1f;
                case SunAnimations.EveningBeforeSunset:
                    if (The.Sim.DateAndTime.SunElevation > sundiskHitsHorizon)
                    {
                        return 1f;
                    }
                    else
                    {
                        progress = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, sundiskHitsHorizon) / sundiskHitsHorizon;
                        return MathHelper.Lerp(1f, 0f, progress);
                    }
                case SunAnimations.EveningAfterSunset:
                    return 0f;
                case SunAnimations.Night:
                    return 0f;
            }

            return 0f;
        }

        public float GetOwnShadowFactor()
        {
            float progress;
            switch (SunAnimation)
            {
                case SunAnimations.MorningBeforeSunrise:
                    progress = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, DateAndTime.dawnSunElevation) / Math.Abs(DateAndTime.dawnSunElevation);
                    return MathHelper.SmoothStep(0f, 0.2f, progress);
                case SunAnimations.MorningAfterSunrise:
                    progress = The.Sim.DateAndTime.SunElevation / DateAndTime.dawnSunElevationEnd;
                    return MathHelper.SmoothStep(0.2f, 1f, progress);
                case SunAnimations.Day:
                    return 1f;
                case SunAnimations.EveningBeforeSunset:
                    progress = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, DateAndTime.sunsetElevationStart) / DateAndTime.sunsetElevationStart;
                    return MathHelper.SmoothStep(1f, 0.4f, progress);
                case SunAnimations.EveningAfterSunset:
                    progress = Math.Abs(The.Sim.DateAndTime.SunElevation / DateAndTime.sunsetElevationEnd); //DateAndTime.Instance.GetEveningAnimationProgress();
                    return MathHelper.SmoothStep(0.4f, 0f, progress);
                case SunAnimations.Night:
                    return 0f;

            }
            return 0f;
        }
    }
}

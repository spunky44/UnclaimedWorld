using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;

namespace UWGame.SimSide.SimEffects
{
    /// <summary>
    /// permanentAdd is for negative morale effects that should increase as normal... Duration can be used to prevent several effect hits in a row.
    /// </summary>
    public enum NumberEffectOperator { Add, Multiply, PermanentAdd }

    public enum AffectsNumbers { AgentComfort, OfferedComfort, Stealth, Need, Morale, NightSensorRange, Detection }

    public class NumberEffectType: EffectType
    {
        /// <summary>
        /// the stat affected by the effect
        /// </summary>
        public AffectsNumbers Affects;

        public NumberEffectOperator Operator;

        /// <summary>
        /// base intensity
        /// </summary>
        public float Intensity;

        /// <summary>
        /// affects display only...
        /// </summary>
        public bool FormatAsPercentage;

          

        public override float? GetIntensity()
        {
            return Intensity;
        }


        public override void AppendAsString(StringBuilder text, Background background)
        {
            string field = GetFieldAsString();

            Common.Append(text, field);
            Common.Append(text, ": ");

            string op = GetOperatorAsString();

            Common.Append(text, op);

            bool useColoring = false;
            if (background == Background.White)
            {
                useColoring = true;
            }

            if (FormatAsPercentage)
            {
                Common.AppendPercentage(text, Intensity, useColoring, null);
            }
            else
            {
                string valueAsString = Common.ValueToDecimalString(Intensity, useColoring, null);
                Common.Append(text, valueAsString);
            }
           
        }

        private string GetOperatorAsString()
        {
            switch (Operator)
            {
                case NumberEffectOperator.Add:
                case NumberEffectOperator.PermanentAdd:
                    if (Intensity < 0f)
                    {
                        return "-";
                    }
                    else
                    {
                        return "";
                    }
                case NumberEffectOperator.Multiply:
                    return "*";

            }

            return "";
        }

        private string GetFieldAsString()
        {
            switch (Affects)
            {
                case AffectsNumbers.AgentComfort:
                    return "Comfort";

                case AffectsNumbers.OfferedComfort:
                    return "Offered comfort";

                case AffectsNumbers.Stealth:
                    return "Stealth";

                case AffectsNumbers.Need:
                    return "Need";

                case AffectsNumbers.Morale:
                    return "Morale";

                case AffectsNumbers.NightSensorRange:
                    return "Night sensor range";

                case AffectsNumbers.Detection:
                    return "Detection";

            }

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.SimEffects
{
    public enum AffectsFlags { CanEmigrate, CanComplain }

    public class FlagEffectType: EffectType
    {
        public AffectsFlags Affects;

        public bool Value;

        public override void AppendAsString(StringBuilder text, Background background)
        {
            string field = GetFieldAsString();

            Common.Append(text, field);
            Common.Append(text, ": ");

            string valueAsString = Common.BoolToString(Value, true);
            Common.Append(text, valueAsString);            

        }

        private string GetFieldAsString()
        {
            switch(Affects)
            {
                case AffectsFlags.CanEmigrate:
                    return "Can emigrate";

                case AffectsFlags.CanComplain:
                    return "Can complain";

            }

            return null;
        }
    }
}

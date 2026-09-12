using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.AllGameData
{
    public class EffectTypeLoader
    {
        public static List<EffectType> Init()
        {
            List<EffectType> list = new List<EffectType>();

            #region Comfort
            list.Add(new NumberEffectType()
            {
                KeyName = "stimulantComfortEffect",
                Affects = AffectsNumbers.AgentComfort,
                FormatAsPercentage = true,
                DurationInDays = 1d,
                Intensity = 0.05f
            });

            list.Add(new NumberEffectType()
            {
                KeyName = "highStimulantComfortEffect",
                Affects = AffectsNumbers.AgentComfort,
                FormatAsPercentage = true,
                DurationInDays = 1d,
                Intensity = 0.08f

            });

            list.Add(new NumberEffectType()
            {
                KeyName = "modestHomeComfortEffect",
                Affects = AffectsNumbers.OfferedComfort,
                FormatAsPercentage = true,
                Intensity = 0.08f

            });

            list.Add(new NumberEffectType()
            {
                KeyName = "smallHomeComfortEffect",
                Affects = AffectsNumbers.OfferedComfort, 
                FormatAsPercentage = true,
                Intensity = 0.05f

            });

            list.Add(new NumberEffectType()
            {
                KeyName = "openStoveComfortEffect",
                Affects = AffectsNumbers.OfferedComfort,
                FormatAsPercentage = true,
                Intensity = -0.02f

            });
            #endregion

            list.Add(new NumberEffectType()
            {
                KeyName = "cloaking",
                Affects = AffectsNumbers.Stealth,
                Operator = NumberEffectOperator.Add,
                Intensity = 0.3f
            });

            list.Add(new NumberEffectType()
            {
                KeyName = "nightVision",
                Affects = AffectsNumbers.NightSensorRange,
                Operator = NumberEffectOperator.Multiply,
                Intensity = 2f
            });

            list.Add(new NumberEffectType()
            {
                KeyName = "groundScanner",
                Affects = AffectsNumbers.Detection,
                //AffectsTypeKey = , //ResourceType key ("torux") or EntityType key ("entity:bushDragon")
                AffectsTypeTag = new[] { "smallAboveGround", "aboveGroundHardToSee" }, // DetectionTag in EntityType or ResourceType
                Operator = NumberEffectOperator.Multiply,
                Intensity = 3f
            });

            list.Add(new FlagEffectType()
            {
                KeyName = "leaderCannotEmigrate",
                Name = "Leader can emigrate", // shown in tooltip on stop sign icon
                Affects = AffectsFlags.CanEmigrate,               
                Value = false
            });

            list.Add(new FlagEffectType()
            {
                KeyName = "leaderCannotComplain",
                Name = "Leader can complain", // not shown currently
                Affects = AffectsFlags.CanComplain,
                Value = false
            });

            return list;

        }

    }
}

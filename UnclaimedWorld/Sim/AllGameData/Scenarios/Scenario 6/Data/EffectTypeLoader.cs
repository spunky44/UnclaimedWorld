using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data
{
    public class EffectTypeLoader
    {
        public static List<EffectType> Init()
        {
            List<EffectType> list = new List<EffectType>();
                       
            list.Add(new FlagEffectType()
            {
                KeyName = "contract",
                Name = "Has permission to leave", // shown in tooltip 
                Affects = AffectsFlags.CanEmigrate,             
                DynamicDurationInDays = new UnaryFunctionNode() { Operator = UnaryExpressionOperator.DateToRelativeDays, Operand = new ValueNode() { PropertyKey = "endDate" } },
                Value = false 
            });

            return list;

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data
{
    public class EffectProfileLoader
    {
        public static List<EffectProfileType> Init()
        {
            List<EffectProfileType> list = new List<EffectProfileType>();
            
            list.Add(new EffectProfileType()
            {
                KeyName = "contract",
                Name = "Contract",
                Description = "The character will not leave the site while under contract.",
                Effects = new string[] { "contract" }

            });

            return list;
        }

    }
}

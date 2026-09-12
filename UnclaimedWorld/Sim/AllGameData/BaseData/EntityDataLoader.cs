using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData
{
    public class EntityDataLoader
    {
        public static List<EntityData> Init()
        {
            List<EntityData> list = new List<EntityData>();


            #region dogs for trade
            list.Add(new EntityData()
            {
                KeyName = "dog",
                EntityKey = "entity:dog",
                BioEntity = new Maps.MapEditor.BiologicalEntity()
                {
                    AgeInYears = new NormalDistribution() { Mean = 4f },
                    CultureTemplates = new StringChance[] { new StringChance() { Edge = 1f, String = "dogCulture" } }
                },
            });

            #endregion


            return list;
        }
    }
}

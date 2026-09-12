using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities
{
  /*  public class MagazineType
    {
        public string AmmoTypeTag;
        public string AmmoTypeKeyName;


        public int MaxCapacity;


        [XmlIgnore]
        public List<EntityType> AmmoEntityTypes;


        public void PostLoadContentInitialize()
        {

            if (!string.IsNullOrEmpty(AmmoTypeTag))
            {
                AmmoEntityTypes = GameData.Instance.AmmoByTag[AmmoTypeTag];
            }

            if (!string.IsNullOrEmpty(AmmoTypeKeyName))
            {
                EntityType ammoType = GameData.Instance.AllEntityTypes[AmmoTypeKeyName];

                if (AmmoEntityTypes == null)
                {
                    AmmoEntityTypes = new List<EntityType>();
                }

                if (!AmmoEntityTypes.Contains(ammoType))
                {
                    AmmoEntityTypes.Add(ammoType);
                }
            }

        }
    }*/
}

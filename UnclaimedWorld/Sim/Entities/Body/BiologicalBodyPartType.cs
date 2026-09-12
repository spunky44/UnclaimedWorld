using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Body
{
    public class BiologicalBodyPartType : BodyPartType
    {
     //   public BodyPartType Parent;

        public OrganType[] OrganTypes;

      
        public string TissueLayer;

       

        [XmlIgnore]
        public BodyLayerType TissueLayerType;

        [XmlIgnore]
        public bool HasVitalOrgans = false;

        //public BodyLayerType[] LayerTypes;

        public override bool IsVital()
        {
            return HasVitalOrgans;
     
        }

        public override void Initialize()
        {
            base.Initialize();

            if (OrganTypes != null)
            {
                foreach (OrganType organ in OrganTypes)
                {
                    if (organ.IsVital)
                    {
                        this.HasVitalOrgans = true;
                        break;
                    }
                }
            }

            

            if (TissueLayer != null)
            {
                TissueLayerType = GameData.Instance.AllBodyLayerTypes[TissueLayer];
            }
        }

        public override void PostLoadContentInitialize()
        {
            base.PostLoadContentInitialize();

            

            if (TissueLayerType != null)
            {
                TissueLayerType.PostLoadContentInitialize();
            }

        }
        
    }
}

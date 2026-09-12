using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Items;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Maps.MapEditor;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AllGameData;
namespace UWGame.SimSide.Trees
{
    public class TreeType : /*IXmlSerializable,*/ IAddonType 
    {
     
        /// <summary>
        /// in years
        /// </summary>
        public float MatureAge;
        public float MaxAge;

        /// <summary>
        /// the bulk value of a tree of size=1 (fullgrown) 
        /// </summary>
        public float BulkPerSize;

        public int MaxFlavours = 1;
      
        /// <summary>
        /// TODO: replace with start and end date of seasons...
        /// </summary>
        public bool HasSummerWinterCycle = false;

       
        public float SizeImpact;

        public string[] Crops;

        [XmlIgnore]
        public List<ResourceType> CropTypes;

        /// <summary>
        /// used to populate trees with default crops - delete after we have real ecology
        /// </summary>
        public DefaultCrops[] DefaultCrops;


        public TreeType() { }


        public float MassLossPercentagePerSecond;

        public float FibrousPercentageOfTotalMass;

        public float LumberPercentageOfFiberMass;

        public float WaterNeedsPerBulk;
        public float NitrogenNeedsPerBulk;
        public float PhosphorousNeedsPerBulk;

        public float BulkGrowthSpeedInSeconds;

        public float SelfSustainmentNeedsInBulkPercentagePerSecond;


        public void Initialize()
        {
            
            if (DefaultCrops != null)
            {
                foreach (var item in DefaultCrops)
                {
                    item.Initialize(); //BulkPerSize);
                }
            }
             
        }

        public void PostLoadContentInitialize()
        {
            if (Crops != null)
            {
                CropTypes = new List<ResourceType>();
                foreach (var item in Crops)
                {
                    CropTypes.Add(GameData.Instance.AllResourceTypes[item]);
                }

            }

          /*  if (CropTypes != null)
            {
                foreach (ResourceType cropType in CropTypes)
                {
                    cropType.PostLoadContentInitialize();
                }
            }*/
        }


      /*  #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }


        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(TreeType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()
        };

       

        #endregion*/
    }
}

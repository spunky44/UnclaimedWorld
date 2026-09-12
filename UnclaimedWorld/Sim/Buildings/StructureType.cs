using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using System.Xml.Serialization;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
namespace UWGame.SimSide.Buildings
{
   /* public struct LightSourceOffset
    {
        public Point Offset;
        public LightSourceType LightSourceType;
    }*/

     

    [XmlRoot("Structure")]
    public class StructureType : IXmlSerializable 
    {
            
        /// <summary>
        /// if false, the structure cannot be
        /// </summary>
        public bool BuildByPlayer = false;


        /// <summary>
        /// NEVER USED???
        /// 
        /// if true, the structure cannot be placed from the inventory panel.
        /// Will be set automatically if the process has output. 
        /// For fish traps, the output is placed via spawn instead for scripting reasons, so it has to be set manually
        /// </summary>       
        public bool UsesAnchor = false;



        /// <summary>
        /// These are copied from TileLayoutType, or 1 if the structure is an edge type...
        /// Deprecate!!
        /// </summary>
        [XmlIgnore]
        public int WidthInTiles;
       

        [XmlIgnore]
        public int HeightInTiles;
       
        
       
        /// <summary>
        /// only build the fireplace addon...
        /// </summary>
        public bool IsCamp = false;


        public bool IsRoad = false;
        
        public bool IsAddon = false;

        /// <summary>
        /// Edge type as well?
        /// </summary>
        [XmlIgnore]
        public AddonSize AddonSize;

        
               
        
        /*
        [XmlIgnore]
        public List<Point> BigAddonSlots; // = new List<Point>();
        [XmlIgnore]
        public List<Point> SmallAddonSlots; // = new List<Point>();
        [XmlIgnore]
        public List<Point> TinyAddonSlots;
        
        public StructureType()
        {
        }*/

        public StructureType() //string keyName)
        {
           // KeyName = keyName;
            // default category:
           // Category = GameData.Instance.AllStructureCategories["miscellaneous"];

           //
        }

       
        public void Initialize(EntityType parent)
        {
            /*
        
            if (IsAddon)
            {
                if (parent.TileLayoutType != null)
                {
                    BuildingTypeData data = GameData.Instance.BillboardSpriteSheet.BuildingTypeData[parent.TileLayoutType.LayoutName];
                    // UWGame.SimSide.Instance.Map.renderer.BillboardSpriteSheet.BuildingTypeData[parent.TileLayoutType.LayoutName]; // SpriteName];

                    if (data.WidthInTiles == 1 && data.HeightInTiles == 1)
                    {
                        AddonSize = AddonSize.Tiny;
                    }
                    else if (data.WidthInTiles == 2 && data.HeightInTiles == 1)
                    {
                        AddonSize = AddonSize.Small;
                    }
                    else if (data.WidthInTiles == 3 && data.HeightInTiles == 2)
                    {
                        AddonSize = AddonSize.Big;
                    }
                }
            }
          
                    
            if (parent.TileLayoutType != null)
            {
                WidthInTiles = parent.TileLayoutType.WidthInTiles;
                HeightInTiles = parent.TileLayoutType.HeightInTiles;
            }
            else
            {
                WidthInTiles = 1;
                HeightInTiles = 1;
            }
                */      
                    
        }


       

        /// <summary>
        /// has to be called after process graph is built
        /// </summary>
        /// <param name="parent"></param>
        public void MarkAnchorStructures(EntityType parent)
        {
            List<ProcessType> processes;
            if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(parent, out processes))
            {
                foreach (var item in processes)
                {
                    if (item.UsesAnchor())
                    {
                        this.UsesAnchor = true;
                        break;
                    }
                }
            }
        }


        public void PostLoadContentInitialize(EntityType parent)
        {
            if (IsAddon)
            {
               /* if (parent.TileLayoutType != null)
                { 
                    // DEPRECATE TILE LAYOUT
                    

                    BuildingTypeData data;

                    ExtendedSpriteSheet sheet = GameData.Instance.BillboardSpriteSheet;

                    if (sheet.BuildingTypeData.TryGetValue(parent.TileLayoutType.LayoutName, out data))
                    {
                        // UWGame.SimSide.Instance.Map.renderer.BillboardSpriteSheet.BuildingTypeData[parent.TileLayoutType.LayoutName]; // SpriteName];

                        if (data.WidthInTiles == 1 && data.HeightInTiles == 1)
                        {
                            AddonSize = AddonSize.Tiny;
                        }
                        else if (data.WidthInTiles == 2 && data.HeightInTiles == 1)
                        {
                            AddonSize = AddonSize.Small;
                        }
                        else if (data.WidthInTiles == 3 && data.HeightInTiles == 2)
                        {
                            AddonSize = AddonSize.Big;
                        }
                    }
                    else
                    {
                        AddonSize = AddonSize.Tiny;//hack hack hack
                    }
                }*/
            }


          /*  if (parent.TileLayoutType != null)
            {
                WidthInTiles = parent.TileLayoutType.WidthInTiles;
                HeightInTiles = parent.TileLayoutType.HeightInTiles;
            }
            else
            {*/
                WidthInTiles = 1;
                HeightInTiles = 1;
           // }
        }

       


        

       

        #region IXmlSerializable Members

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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(StructureType))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    new CustomXmlSerializer.XmlTypeMapping<EntityType, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                        SetterMethod = s => s == null ? null : GameData.Instance.AllEntityTypes[s]
                    } 
                    /* ,
                     new CustomXmlSerializer.XmlTypeMapping<StructureCategory, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                        SetterMethod = s => s == null ? null : GameData.Instance.AllStructureCategories[s]
                    }
                   GetMaterialInputSerializer(true)    */ 
                }
        };

        #endregion

       
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using UWGame.SimSide.Vegetation;

namespace UWGame.SimSide.Soil //TODO DECOUPLE -- move this to client
{

    public class SoilComponentType : RenderedTerrainType 
    {
        public string Name;
       // private string keyName;

        /// <summary>
        /// 0: flat and firm
        /// 1: maximum rugged
        /// TODO: create a Sim class for this DECOUPLE
        /// </summary>
        public float MoveFactor = 0f;
        
        public RenderAsRocksType RenderAsRocksType;

        public bool ScaleDisplayAmountAsWithRocks = false;

        
        
        private Color dryTint;
        public Color DryTint
        {
            get { return dryTint; }
            set
            {
                dryTint = value;
                DryTintAsVector = dryTint.ToVector4();
                DryTintAsVector.W = 1f;
            }
        }

        private Color wetTint;
        public Color WetTint
        {
            get { return wetTint; }
            set 
            { 
                wetTint = value;
                WetTintAsVector = wetTint.ToVector4();
                WetTintAsVector.W = 1f;
            }
        }

        [XmlIgnore]
        public Vector4 WetTintAsVector;

        [XmlIgnore]
        public Vector4 DryTintAsVector;
                
        public SoilComponentType() 
        {
            WetTint = new Color(240, 240, 240, 255);
            DryTint = new Color(255, 255, 255, 255);
        }

        public SoilComponentType(string keyName): this()
        {
            this.KeyName = keyName;
            
        }



        public void PostLoadContentInitialize()
        {
            if (RenderAsRocksType == null)
            {
                RenderOrder = RenderedTerrainType.HighestOrder;
                RenderedTerrainType.HighestOrder++;
            }
            else
            {
                // kvp.Value.RenderOrder = 0; // don't care
                RenderOrder = RenderedTerrainType.HighestOrder + 100;
                RenderedTerrainType.HighestOrder++;
            }

        }

        #region IDataType Members

        

        #endregion
    }
}

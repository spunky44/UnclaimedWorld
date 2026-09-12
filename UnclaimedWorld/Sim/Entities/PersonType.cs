using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Maps;
using System.Xml.Serialization;
using GameStateManagement;
using UWGame.Control;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities //TODO DECOUPLE, PersonType appears to be about color only. Move to RenderableType
{
    public class PersonType : IXmlSerializable 
    {
              
        /*
          I very light, also "Celtic" [16] Often burns, rarely tans. Tends to have freckles, red or blond hair, blue or green or gray eyes. 1-5 
          * 
         II light, or light-skinned European[16] Usually burns, sometimes tans Tends to have light or dark hair, blue or green or hazel or brown or gray eyes. 6-10 
          * 
         III light intermediate, or dark-skinned European or "average Caucasian"[16] Sometimes burns, usually tans. Usually has brown hair and blue, green, hazel, or brown eyes. 11-15 
          * 
         IV dark intermediate, also "Mediterranean" or "Olive skin"[16] Sometimes burns, often tans. Tends to have dark brown eyes and hair. 16-20 
          * 
         V dark or "Brown" type Naturally black-brown skin Often has black- brown eyes and hair. 21-28 
          * 
         VI very dark, or "Black" type Naturally black-brown skin Usually has black-brown eyes and hair. 29-36          
          */

        public enum SkinColors { Celtic = 0, LightEuropean = 1, AverageCaucasian = 2, OliveSkin = 3, Dark = 4, Black = 5 }
        public enum HairColors { Platinum = 0, LightBlonde = 1, Red = 2, MediumBlonde = 3, DarkBlonde = 4, LightBrown = 5, DarkBrown = 6, DarkestBrown = 7, Black = 8, Grey = 9, White = 10, Bald = 11 }

      //  public float[] SkinColorProbabilities; 

        // the probabilities for hair color by kind of skin 
       /* public Dictionary<SkinColors, float[]> HairColorProbabilities;  = new Dictionary<SkinColors, float[]>() { 
                    {SkinColors.Celtic, new float[] { 0.1f, 0.5f, 0.9f, 1f } },
                    {SkinColors.LightEuropean, new float[] { 0.1f, 0.2f, 0.3f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f }},
                    {SkinColors.AverageCaucasian, new float[] { 0f, 0f, 0f, 0.1f, 0.2f, 0.7f, 0.8f, 0.9f, 1.0f }},
                    {SkinColors.OliveSkin, new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0.1f, 0.7f, 0.9f, 1.0f }},
                    {SkinColors.Dark, new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.2f, 0.7f, 1.0f }},
                    {SkinColors.Black, new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f }}        
        }; */

        
   /*     [XmlIgnore]
        public Dictionary<HairColors, Vector3> HairColorsForRendering = new Dictionary<HairColors, Vector3>();

        [XmlIgnore]
        public Dictionary<SkinColors, Vector3> SkinColorsForRendering = new Dictionary<SkinColors, Vector3>();
        */

     /*   public List<string> ShirtColorsHex;
        public List<string> PantsColorsHex;
        */
     //   [XmlIgnore]

        public List<Vector3> ShirtColors = new List<Vector3>();
    //    [XmlIgnore]

        public List<Vector3> PantsColors = new List<Vector3>();

        /// <summary>
        ///  Enables SOCIAL EATING - wastes less food now that leftovers are produced
        /// </summary>
     //   public bool AlwaysEatAtDinnerTime = false;


            //Dictionary<float, HairColors>> 

   //     public static Vector3[] SkinColors = new Vector3[] { new Vector3() };

        public PersonType()
           // : base("person")
        {
         /*   Name = "Person";
            ModelName = "man";

            BaseSpeed = 40f;
            FleeingSpeed = 63f;
            WanderSpeed = 21f;
            ModelScale = 1.5f;
            ModelOffset = new Vector3(-158f, 0f, 0f);

            MaxAngularVelocity = MathHelper.Pi;

            ThreatCategory = ThreatCategory.Human;

            BaseSpeedOverStandardTerrain = BaseSpeed / (float)PlainsType.Instance.Cost(TerrainType.TransportType.Foot, TerrainType.TerrainFeatures.None);
           
            sourceRect = new Rectangle(0, 0, 48, 48);            
             */                                                        
            
        }

        public void Initialize()
        {
                                                                        // Upper Ranges
          /*  HairColorProbabilities = new Dictionary<SkinColors, float[]>();   //     Pt  LiBl   Red  MedBl DaBl  LiBr  DaBr  DkBr    Bl     
            HairColorProbabilities.Add(SkinColors.Celtic, new float[] { 0.1f, 0.5f, 0.9f, 1f });
            HairColorProbabilities.Add(SkinColors.LightEuropean, new float[] { 0.1f, 0.2f, 0.3f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f });
            HairColorProbabilities.Add(SkinColors.AverageCaucasian, new float[] { 0f, 0f, 0f, 0.1f, 0.2f, 0.7f, 0.8f, 0.9f, 1.0f });
            HairColorProbabilities.Add(SkinColors.OliveSkin, new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0.1f, 0.7f, 0.9f, 1.0f });
            HairColorProbabilities.Add(SkinColors.Dark, new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.2f, 0.7f, 1.0f });
            HairColorProbabilities.Add(SkinColors.Black, new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f, 1.0f });
            */

            /*
            foreach (string color in ShirtColorsHex)
            {
                ShirtColors.Add(HexStringToVector3(color));
            }

            foreach (string color in PantsColorsHex)
            {
                PantsColors.Add(HexStringToVector3(color));
            }*/

         /*   HairColorsForRendering.Add(HairColors.Platinum, HexStringToVector3("EEE6DB"));
            HairColorsForRendering.Add(HairColors.LightBlonde, HexStringToVector3("E4D4B3"));
            HairColorsForRendering.Add(HairColors.Red, HexStringToVector3("C58643"));
            HairColorsForRendering.Add(HairColors.MediumBlonde, HexStringToVector3("C8AA88"));
            HairColorsForRendering.Add(HairColors.DarkBlonde, HexStringToVector3("A78061"));
            HairColorsForRendering.Add(HairColors.LightBrown, HexStringToVector3("836853"));
            HairColorsForRendering.Add(HairColors.DarkBrown, HexStringToVector3("633C35"));
            HairColorsForRendering.Add(HairColors.DarkestBrown, HexStringToVector3("403030"));
            HairColorsForRendering.Add(HairColors.Black, HexStringToVector3("171518"));
            HairColorsForRendering.Add(HairColors.Grey, HexStringToVector3("CBCCC6"));
            HairColorsForRendering.Add(HairColors.White, HexStringToVector3("E7E6E2"));

            SkinColorsForRendering.Add(SkinColors.Celtic, HexStringToVector3("FAF9F7"));
            SkinColorsForRendering.Add(SkinColors.LightEuropean, HexStringToVector3("F3EAE5"));
            SkinColorsForRendering.Add(SkinColors.AverageCaucasian, HexStringToVector3("FEF6E1"));
            SkinColorsForRendering.Add(SkinColors.OliveSkin, HexStringToVector3("EBD69F"));
            SkinColorsForRendering.Add(SkinColors.Dark, HexStringToVector3("9C6B43"));
            SkinColorsForRendering.Add(SkinColors.Black, HexStringToVector3("573229"));
            */

            // browns
    /*        ShirtColors.Add(HexStringToVector3("7F6F57"));
            ShirtColors.Add(HexStringToVector3("9E7840"));

            // greens
            ShirtColors.Add(HexStringToVector3("5B9E62"));
            ShirtColors.Add(HexStringToVector3("82AD7E"));

            //blues
            ShirtColors.Add(HexStringToVector3("499EFF"));
            ShirtColors.Add(HexStringToVector3("518DAD"));

            // reds
            ShirtColors.Add(HexStringToVector3("BC4240"));

            // grays
            ShirtColors.Add(HexStringToVector3("C6C6C6"));
            ShirtColors.Add(HexStringToVector3("7A7A7A"));

            // blacks
            ShirtColors.Add(HexStringToVector3("565656"));
            // blacks
            ShirtColors.Add(HexStringToVector3("636363"));


            // browns
            PantsColors.Add(HexStringToVector3("705D45"));
            PantsColors.Add(HexStringToVector3("A0896D"));

            // greens
            PantsColors.Add(HexStringToVector3("618262"));

            //blues
            PantsColors.Add(HexStringToVector3("426FA5"));
            PantsColors.Add(HexStringToVector3("7DB8E0"));

            // grays
            PantsColors.Add(HexStringToVector3("7F7F7F"));
            PantsColors.Add(HexStringToVector3("A0A0A0"));
           */
        }

        public void UpdateRenderableRandomColors(Renderable renderable)
        {
            renderable.RenderAsModel.CustomColor0 = GetRandomPants(); // pants
            renderable.RenderAsModel.CustomColor1 = GetRandomShirt(); // shirt

        }


        public static string FormatVector3(Vector3 v)
        {
            string s = string.Format("new Vector3({0}f# {1}f# {2}f)", v.X, v.Y, v.Z);
            s = s.Replace(",", ".");
            s = s.Replace("#", ",");
            return s;
        }

        public static Vector3 HexStringToVector3(string hex)
        {
            int red = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            int green = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            int blue = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            return new Vector3((float)red / 255f, (float)green / 255f, (float)blue / 255f);

        }

        public static string Vector3ToHexString(Vector3 vector)
        {
            return string.Format("{0}{1}{2}", ((int)(255f * vector.X)).ToString("X2"), ((int)(255f * vector.Y)).ToString("X2"), ((int)(255f * vector.Z)).ToString("X2"));           
        }
        

     /*   public SkinColors GetRandomSkinColor()
        {
            return (SkinColors) Common.GetStairStepIndex((float)Globals.Instance.Random.NextDouble(), SkinColorProbabilities);
        }*/

        public Vector3 GetRandomShirt()
        {
            return ShirtColors[The.Sim.GameplayRandomGenerator.Next(ShirtColors.Count,"PersonType")];
        }

        public Vector3 GetRandomPants()
        {
            return PantsColors[The.Sim.GameplayRandomGenerator.Next(PantsColors.Count, "PersonType")];
        }

    /* OLD:
     * public HairColors GetHairColor(SkinColors skin, bool isAsian, float age)
        {
            if (age > 58f && age < 70f)
            {
                return HairColors.Grey;
            }
            else if (age >= 70f)
            {
                return HairColors.White;
            }

            if (isAsian) // east asians always black hair?
            {
                return HairColors.Black;
            }

            float roll = (float)Globals.Instance.Random.NextDouble();
            float[] hairColors = HairColorProbabilities[skin];

            return (HairColors) Common.GetStairStepIndex(roll, hairColors);
        }*/

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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(PersonType))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                  /*  new CustomXmlSerializer.XmlTypeMapping<Vector3, string> ()
                    {
                        GetterMethod = t => PersonType.Vector3ToHexString(t),
                        SetterMethod = s => PersonType.HexStringToVector3(s)
                    }       */ 
                //  new CustomXmlSerializer.XmlTypeMapping<List<Vector3>, List<string>> ()
                // MUST be array type!!!
                  new CustomXmlSerializer.XmlTypeMapping<List<Vector3>, string[]> ()
                    {
                      
                        GetterMethod = t => {
                            if (t == null) return null;
                            //var d = new List<string>();
                            var d = new string[t.Count];
                            int index = 0;
                            foreach (Vector3 color in t) 
	                        {
                        	   // d.Add(PersonType.Vector3ToHexString(color));	
                                d[index] = PersonType.Vector3ToHexString(color);	
                                index++;
	                        }
                            return d;                            
                        },

                      SetterMethod = t => {
                            if (t == null) return null;
                            var d = new List<Vector3>();
                          //  int index = 0;
                            foreach (string color in t) 
	                        {
                        	    d.Add(PersonType.HexStringToVector3(color));	
                              //  index++;
	                        }
                            return d;                            
                        },
                    }     
                }

           /* TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {                  

                  new CustomXmlSerializer.XmlTypeMapping<Dictionary<SkinColors, float[]>, KeyValuePair[]>()
                    {
                        GetterMethod = t => {
                            if (t == null) return null;
                            var d = new KeyValuePair[t.Count];
                            int index = 0;
                            foreach (KeyValuePair<SkinColors, float[]> kvp in t)
	                        {
                        	    d[index] = new KeyValuePair() {Key = kvp.Key, Value = kvp.Value};	
                                index++;
	                        }
                            return d;                            
                        },

                        SetterMethod = s => { 
                            if (s == null) return null; 
                            var d = new Dictionary<SkinColors, float[]>(); 
                            foreach (var el in s) 
                            { 
                                d[el.Key] = el.Value; 
                            } 
                            return d; 
                        }
                    }                   
                }*/
        };

        #endregion
            


               

    }

    public class KeyValuePair
    {
        public PersonType.SkinColors Key { get; set; }
        public float[] Value { get; set; }
    }
}

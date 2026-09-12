using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Combat;
namespace UWGame.SimSide.Items
{

    
    public class WeaponType: IXmlSerializable 
    {
       
        //A weapon can have a set of attack types like throw/stab... shoot/club
        public AttackType[] AttackTypes;

      /*  public DefenseRatings? DefenseRatingWithAmmo;
        public DefenseRatings? DefenseRatingNoAmmo;
        */
       

        public bool? IsIntrinsic;

        [XmlIgnore]
        public string HighlyEffectiveAgainst;

        [XmlIgnore]
        public string NotEffectiveAgainst;


        public WeaponType()
        {
           
        }

        public float GetHighestDefenseRating()
        {
            float maxRating = AttackTypes.Max(a => (a.DefenseRating ?? 0f));

            return maxRating;
        }
        /*
        public  DefenseRatings GetHighestDefenseRating()
        {
            int maxRating = AttackTypes.Max(a => (int)(a.DefenseRating ?? DefenseRatings.None));

            return (DefenseRatings)maxRating;
        }*/

        public void PostLoadContentInitialize()
        {
            
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(WeaponType)) 
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true)
        };

        #endregion
    }
}

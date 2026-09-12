using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Trade;

namespace UWGame.SimSide.AllGameData
{
    public class OfferDemandProfileLoader
    {
        public static List<OfferDemandProfile> Init()
        {
            List<OfferDemandProfile> list = new List<OfferDemandProfile>();

            #region 
         
            list.Add(new OfferDemandProfile()
            {
                KeyName = "occasionallyOfferedGood", 

                NonlinearAmountForSale = new Systems.NoiseParams()
                {
                     NoiseAmplitude = 2f,
                     NoiseAddend = -0.25f,
                     NoiseFrequency = 0.005f 
                }
                
                /*
                 States = new []
                 {   // not too sure how much different this behaves than a normal bell curve with mean = 0...
                     new OfferDemandState() { Edge = 0.2f, OfferDemandChange = new NormalDistribution() { Mean = 1, StandardDeviation = 0.2f } },
                     new OfferDemandState() { Edge = 0.4f, OfferDemandChange = new NormalDistribution() { Mean = -1, StandardDeviation = 0.2f } },                   
                     new OfferDemandState() { Edge = 1f, OfferDemandChange = new NormalDistribution() { Mean = 0 } }
                 }*/
                       
            });
            #endregion

            #region
            list.Add(new OfferDemandProfile()
            {
                KeyName = "occasionallyOfferedGoodHigherQuantity",

                NonlinearAmountForSale = new Systems.NoiseParams()
                {
                     NoiseAmplitude = 4f,
                     NoiseAddend = -0.25f,
                     NoiseFrequency = 0.005f 
                }

                /*
                States = new[]
                 {
                     new OfferDemandState() { Edge = 0.2f, OfferDemandChange = new NormalDistribution() { Mean = 2, StandardDeviation = 0.5f } },
                     new OfferDemandState() { Edge = 0.4f, OfferDemandChange = new NormalDistribution() { Mean = -2, StandardDeviation = 0.5f } },                   
                     new OfferDemandState() { Edge = 1f, OfferDemandChange = new NormalDistribution() { Mean = 0 } }
                 }*/

            });
            #endregion

            return list;
        }

    }
}

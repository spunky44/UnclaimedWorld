using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.Overland.Templates
{
   /// <summary>
   /// the allegiance is fairly uninteresting.
   /// except for the ratings, which influence migration...
   /// 
   /// Suggested types: Poor, average, wealthy, wartorn, hunger..?
   /// Expeditions with their trade goods will then also depend on the description and ratings here.
   /// </summary>
    public class AllegianceTemplate: IGameData
    {
        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public bool DeleteRecord
        {
            get;
            set;
        }

        /// <summary>
        /// edges/buckets are allowed.
        /// 
        /// keys are to ExpeditionData/Template?
        /// 
        /// multiple expeditions!
        /// </summary>
        public StringChanceSet[] Expeditions;


        // ratings here?


        public void FillAllegiance(Allegiance allegiance, float? sizeFactor, string expeditionKeyName = null)
        {
            if (Expeditions != null)
            {
                foreach (var item in Expeditions)
                {
                    int index;
                    StringChance trait = Common.GetStairStepIndex(item.Chances, out index, The.Sim.GameplayRandomGenerator);

                    ExpeditionData expeditionData = GameData.Instance.AllExpeditionData[trait.String];

                    string failReason;
                    Expedition.CreateFromExpeditionData(expeditionData, allegiance, null, sizeFactor, expeditionKeyName, out failReason);
                   // Expedition expedition = Expedition.CreateFromExpeditionData(expeditionData);

                }
            }

           /* if (allegiance.Name == null)
            {
                allegiance.Name = allegiance.Site.Name; // set a default name
            }*/
        }     
     

        public void PreInitValidate(ref List<string> errors)
        {
          
        }

        public void Initialize()
        {
           
        }

        public void PostInitValidate(ref List<string> errors)
        {
          
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {
           
        }

        public void PostDataCompleteInitialize()
        {
            
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
           
        }
    }
}

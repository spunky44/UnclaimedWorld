using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Policies;

namespace UWGame.SimSide.Allegiances
{
    /// <summary>
    /// defines the state of a non-player allegiance on startup - after that, the class is not used anymore.
    /// 
    /// Randomize these!
    /// Suggested types: Poor, average, wealthy, wartorn, hunger..?
    /// Expeditions with their trade goods will then also depend on the description and ratings here.
    /// </summary>
    public class AllegianceData: IGameData
    {
        public string Name { get; set; }
        public string KeyName { get; set; }

        public bool DeleteRecord
        {
            get;
            set;
        }

       
       // public string AllegianceKeyName { get; set; }


        public string Site;
        public string EntityType;
        public AllegianceType AllegianceType;

        #region Playsite properties

        public int? ForageAndHuntingRadius;

        #endregion

        /// <summary>
        /// only used by othersite allegiances
        /// </summary>
        public StatsData StatsData;

        public AllegiancePolicyData PolicyData;

        public bool PermitsImmigration;


        public StringChance[] AllegianceTemplates;

        /// <summary>
        /// edges/buckets are allowed.
        /// 
        /// keys are to ExpeditionData
        /// 
        /// multiple expeditions: array of arrays!
        /// </summary>
       // public StringChance[][] Expeditions;



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

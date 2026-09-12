using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.InGameEvents.SpecialEvents
{
    /// <summary>
    /// a fixed polled event that can be customized via scenario properties
    /// </summary>
    public class Funeral: ISnapshot
    {


        private EventActionDialog funeralEvent;
        private AgentCondition canParticipateInFuneral;


        /// <summary>      
        /// store any deaths here until it makes sense to show 'funeral'
        /// </summary>
        private List<PlayerEntityDeath> PlayerEntityDeaths = new List<PlayerEntityDeath>();



        public Funeral()
        {

           /* if (site.IsPlaySite)
            {*/
            funeralEvent = new EventActionDialog()
            {               
                DisplayText = new DynamicText() 
                { 
                    SubstitutionValues = new[]
                    {
                        new SubstituteValue() { Placeholder = "#JOURNALNAMES", PropertyName = "getJournalHeaderNames" }
                    }                        
                }
                
            };

            canParticipateInFuneral = UnhappinessGroupMeetingEvent.GetMeetingParticipantConditions();

            /*
                canParticipateInFuneral = new AgentCondition()
                {
                    AllowEmigrating = false,
                    AllowFighting = false,
                    AllowTravelling = false,
                    AllowSleeping = false,
                    AllowThreatened = false,
                    AllowUnconscious = false
                };
          */

        }

        /// <summary>
        /// triggers some burial dialog and destroys the corpse...
        /// </summary>
        public void PlayerEntityHasDied(Entity deadEntity, Entity carcassEntity, CauseOfDeath? causeOfDeath/*,
            string hisHerIts*/)
        {
            
            PlayerEntityDeaths.Add(
                new PlayerEntityDeath()
                {
                    EntityName = deadEntity.Name,
                    Corpse = carcassEntity.EntityID,
                    CauseOfDeath = causeOfDeath,
                    HisHerIts = deadEntity.HisHerOrIts(), // hisHerIts,
                    DisplayImageName = deadEntity.PersonEntity.ComposePortraitKey(),
                    Profession = deadEntity.Intelligence.Profession
                });

        }


       /* void CreateRegulators()
        {
            updateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1f / 10f, "UnhappinessGroupMeetingEvent");
        }*/


        public void Update() //GameTime gameTime)
        {
            HandlePlayerDeaths();

        }

        private string SubstituteBurialText(string text)
        {
            Entity euologyGiver = The.Sim.PlaySite.PlayerAllegiance.GetRandomPerson(e => this.canParticipateInFuneral.IsFulfilled(e)); // .GetRandomName().Value.StringResult; 
            string nameOfEuologyGiver = "";
            if (euologyGiver != null)
            {
                nameOfEuologyGiver = SubstituteValue.FormatAllegianceMember(euologyGiver); // euologyGiver.Name;
            }

            string causeOfDeath = "";
            string nameOfDeceased;

            if (PlayerEntityDeaths.Count == 1)
            {
                PlayerEntityDeath death = PlayerEntityDeaths[0];

                nameOfDeceased = SubstituteValue.FormatEntity(death.EntityName, death.Profession, true); // death.EntityName;

                // let's include the cause of death:               
                if (death.CauseOfDeath != null)
                {
                    switch (death.CauseOfDeath.Value)
                    {
                        case CauseOfDeath.Starvation:
                            causeOfDeath = " who perished from starvation";
                            break;
                        case CauseOfDeath.Wounds:
                            causeOfDeath = " who died from " + death.HisHerIts + " wounds";
                            break;
                        default:
                            causeOfDeath = "";
                            break;
                    }
                }
            }
            else
            {
               // nameOfDeceased = Common.ListToCommaSeparatedString(PlayerEntityDeaths, e => e.EntityName);
                 nameOfDeceased = Common.ListToCommaSeparatedString(PlayerEntityDeaths, e => SubstituteValue.FormatEntity(e.EntityName, e.Profession, true));
            }

            // perform substitution:
            text = text.Replace("#NAMEOFDECEASED", nameOfDeceased);
            text = text.Replace("#EUOLOGYGIVER", nameOfEuologyGiver);
            text = text.Replace("#CAUSEOFDEATH", causeOfDeath);

            return text;
        }

        private string ComposeBurialEventMainText()
        {
            string mainText = "";

            PropertyResult? includeDateInBurialHeader = The.Sim.PlaySite.GetPropertyValue("includeDateInBurialHeader", null);
            // reads text from global script variables
            PropertyResult? burialTextStart = The.Sim.PlaySite.GetPropertyValue("burialTextStart", null);  // "Date: 03-10 2238 \nLocation: 4° 12' 22'' South, 7° 12' 19.2'' West \n \nJournal entry #2 \n{0} \n \n";
            // these are optional...
            PropertyResult? multipleDeathsMultipleSurvivors = The.Sim.PlaySite.GetPropertyValue("burialTextMultipleDeathsMultipleSurvivors", null); // "We have lost #NAMEOFDECEASED. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace."  
            PropertyResult? singleDeathMultipleSurvivors = The.Sim.PlaySite.GetPropertyValue("burialTextSingleDeathMultipleSurvivors", null);  // "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace."                      
            PropertyResult? singleDeathSingleSurvivor = The.Sim.PlaySite.GetPropertyValue("burialTextSingleDeathSingleSurvivor", null); // "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..."
            PropertyResult? multipleDeathsSingleSurvivor = The.Sim.PlaySite.GetPropertyValue("burialTextMultipleDeathsSingleSurvivor", null); // "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..."

            if (The.Sim.PlaySite.PlayerAllegiance.Members.Count > 1)
            {
                if (PlayerEntityDeaths.Count == 1)
                {

                    mainText = GetBurialText(singleDeathMultipleSurvivors, singleDeathSingleSurvivor);
                    /*  if (!string.IsNullOrEmpty(mainText))
                      {
                          // #NAMEOFDECEASED
                          // #CAUSEOFDEATH
                          // #NAMEOFEULOGYGIVER
                       
                          // mainText = string.Format("We have lost {0}{1}. At the burial a eulogy was delivered by {2}. The speech is included as an audio file. \n \nGoodbye, {0}. You will be remembered as a shining example for future generations of settlers. Rest in peace.", PlayerEntityDeaths[0].EntityName, causeOfDeath, nameOfEuologyGiver);
                          mainText = string.Format(mainText, PlayerEntityDeaths[0].EntityName, causeOfDeath, nameOfEuologyGiver);
                      }*/
                }
                else
                {
                    // string names = Common.ListToCommaSeparatedString(PlayerEntityDeaths, e => e.EntityName);

                    mainText = GetBurialText(multipleDeathsMultipleSurvivors, multipleDeathsSingleSurvivor);
                    /*  if (!string.IsNullOrEmpty(mainText))
                      {
                         // mainText = string.Format("We have lost {0}. At the burial a eulogy was delivered by {1}. The speech is included as an audio file. \n \nGoodbye, {0}. You will be remembered as a shining example for future generations of settlers. Rest in peace.", names, nameOfEuologyGiver);
                          mainText = string.Format(mainText, names, nameOfEuologyGiver);
                      }*/

                }
            }
            else
            {
                // one guy left...
                if (PlayerEntityDeaths.Count == 1)
                {

                    mainText = GetBurialText(singleDeathSingleSurvivor, singleDeathMultipleSurvivors);
                    /* if (!string.IsNullOrEmpty(mainText))
                     {
                        // mainText = string.Format("{0} is also dead now. \nI buried the remains as best I could. Guess it's only me now...", PlayerEntityDeaths[0].EntityName);
                         mainText = string.Format(mainText, PlayerEntityDeaths[0].EntityName);
                     }*/
                }
                else
                {
                    //string names = Common.ListToCommaSeparatedString(PlayerEntityDeaths, e => e.EntityName);

                    mainText = GetBurialText(multipleDeathsSingleSurvivor, multipleDeathsMultipleSurvivors);
                    /* if (!string.IsNullOrEmpty(mainText))
                     {
                       //  mainText = string.Format("{0} is also dead now. \nI buried the remains as best I could. Guess it's only me now...", names);
                         mainText = string.Format(mainText, names);
                     } */
                }
            }


            string headerText = "";
            if (burialTextStart != null && !string.IsNullOrEmpty(burialTextStart.Value.StringResult))
            {
                if (includeDateInBurialHeader != null && includeDateInBurialHeader.Value.BoolResult == true)
                {
                    // include the date also:
                    headerText = SubstituteValue.FormatDate(The.Sim.DateAndTime.CurrentTimeDateYear, FormattingOptions.BothDates) + " \n"; //.GetDateForJournal() + " \n \n";
                }

                // append a common starting text ("Date: 03-10 2238 \nLocation: 4° 12' 22...") if it was defined.
                headerText += burialTextStart.Value.StringResult;
            }

            return SubstituteBurialText(headerText + mainText);


        }


        private string GetBurialText(PropertyResult? result, PropertyResult? alternativeResult)
        {
            if (result.HasValue && !string.IsNullOrEmpty(result.Value.StringResult))
            {
                return result.Value.StringResult;
            }
            else if (alternativeResult.HasValue && !string.IsNullOrEmpty(alternativeResult.Value.StringResult))
            {
                return alternativeResult.Value.StringResult;
            }

            return ""; // it is possible to use a constant text only (defined as the startingText)
        }

        /// <summary>
        /// should this handle all dead persons, NPCs too?
        /// </summary>
        private void HandlePlayerDeaths()
        {
            if (funeralEvent == null)
                return;

            if (PlayerEntityDeaths.Count > 0)
            {
                if (OKToShowFuneral())
                {

                    string displayText = ComposeBurialEventMainText();

                    if (displayText != null)
                    {
                        // only show the dialog if there was any text result:

                        // assign text with placeholders:
                        funeralEvent.DisplayText.Text = displayText;

                        if (PlayerEntityDeaths.Count > 1)
                        {
                            funeralEvent.DisplayImage = "NightTime"; // TODO: make img that shows mass  grave lol
                        }
                        else if (PlayerEntityDeaths.Count == 1)
                        {
                            funeralEvent.DisplayImage = PlayerEntityDeaths[0].DisplayImageName;
                        }

                        // perform final text substitution:
                        string failReason = null;
                        funeralEvent.Execute(null, ref failReason);
                    }

                    RemoveDeadPlayerEntities();

                    // now set a flag that indicates that the burial has taken place - this can easily be polled by global events
                    The.Sim.PlaySite.SetPropertyValue("burialOccurred", new PropertyResult() { BoolResult = true });
                }
            }
        }

        private void RemoveDeadPlayerEntities()
        {
            // remove corpses:
            for (int i = 0; i < PlayerEntityDeaths.Count; i++)
            {
                EntityID entityID = PlayerEntityDeaths[i].Corpse;

                DetectableID? detectableID = null;

                // and the entity itself:
                Entity corpse = Entity.FindByID(entityID);
                if (corpse != null)
                {
                    detectableID = corpse.DetectableID;
                    corpse.Destroy(true);
                }

                // let's destroy the remembered fact (body?) also:
                The.Sim.PlaySite.PlayerAllegiance.SharedKnowledge.DeleteMemoryOfEntity(entityID, detectableID, true);

            }

            PlayerEntityDeaths.Clear();
        }


        /// <summary>
        /// we don't want to show funeral dialogs in the middle of a battle. wait until things have quieted down.
        /// </summary>
        /// <returns></returns>
        private bool OKToShowFuneral()
        {

            // don't have a funeral while there are threats around
            if (The.Sim.PlaySite.PlayerAllegiance.IsUnderThreat())
            {
                return false;
            }


            // people must be awake and conscious to arrange funerals
            //bool oneMemberIsAwake = false;
            int siteMembers = The.Sim.PlaySite.PlayerAllegiance.GetNoOfPersons(e => canParticipateInFuneral.IsFulfilled(e));

            return siteMembers > 0;

            /*  foreach (var member in The.Sim.PlaySite.PlayerAllegiance.Members) // .GetNoOfPersons()
              {
                  if (member.EntityType.Person != null // no robots!
                      && member.IsAwakeAndActive())
                  {
                      oneMemberIsAwake = true;
                      break;
                  }
              }

              if (!oneMemberIsAwake)
              {
                  return false;
              }

              return true;*/
        }

         

        
       





        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.PlayerEntityDeaths = (List<PlayerEntityDeath>)sn.DoList(PlayerEntityDeaths);


            sn.Ignore(funeralEvent);
            sn.Ignore(canParticipateInFuneral);


            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


            //CreateRegulators();
        }

        #endregion
    }
}

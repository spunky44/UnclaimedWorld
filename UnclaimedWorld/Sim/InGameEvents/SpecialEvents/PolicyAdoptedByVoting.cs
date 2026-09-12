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
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Tiers;

namespace UWGame.SimSide.InGameEvents.SpecialEvents
{
    
    public class PolicyAdoptedByVoting //: ISnapshot
    {

        /*
           
         *  GROUP VOTE
            PERSON FOR: bla bla bla
            PERSON AGAINST: bla bla
            Proposal adopted: Advance to FOOD BASIC tier. FOOD Principles raised to minimum 16.
            Vote against (-10 Happiness): Lehner, Yeboah
            Vote for: Koppel, Pedersen, Larsen
             
         */

        private EventActionDialog meetingEvent;
      //  private AgentCondition canParticipateInMeeting;
       
       
      
      //  Dictionary<RatingTypes, List<Entity>> PersonsFor = new Dictionary<RatingTypes, List<Entity>>();
      //  Dictionary<RatingTypes, List<Entity>> ContentPersons = new Dictionary<RatingTypes, List<Entity>>();

       // List<Entity> MeetingParticipants = new List<Entity>();

       // Regulator updateRegulator;


        public PolicyAdoptedByVoting()
        {

            meetingEvent = new EventActionDialog()
            {
               
            };

            /*
            canParticipateInMeeting = GetMeetingParticipantConditions();

            if (!Snapshotter.IsSnapshotting)
            {
                
                CreateRegulators();
            }   */
        }

        /*
        public static AgentCondition GetMeetingParticipantConditions()
        {
            return new AgentCondition()
            {
                AllowEmigrating = false,
                AllowFighting = false,
                AllowTravelling = false,
                AllowSleeping = false,
                AllowThreatened = false,
                AllowUnconscious = false
            };
        }*/


       /* void CreateRegulators()
        {
            updateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1f / 10f, "UnhappinessGroupMeetingEvent");
        }*/

       
        

        public void ShowPolicyAdoption(TierArea area, List<Entity> personsFor, List<Entity> personsAgainst, List<Entity> personsNotParticipating)
        {

            string displayText = ComposeMainText(area, personsFor, personsAgainst);

            // only show the dialog if there was any text result:

            if (!string.IsNullOrEmpty(displayText))
            {
                string summaryText = ComposeSummaryText(area, personsFor, personsAgainst, personsNotParticipating);

                displayText += summaryText;

                meetingEvent.Heading = "POLICY ADOPTED";
                meetingEvent.DisplayText = new DynamicText();
                meetingEvent.DisplayText.Text = displayText;
                meetingEvent.DisplayImage = "GroupMeeting"; 
               

                string failReason = null;
                meetingEvent.Execute(null, ref failReason);
            }


           /* totalTimepointInSecondsOfLastMeeting = The.Sim.TotalUnPausedGameTimeInSeconds;
            timepointInSecondsOfLastMeeting[expedition.ID] = The.Sim.TotalUnPausedGameTimeInSeconds;

            PersonsFor.Clear();
            MeetingParticipants.Clear();*/
        }




        private string ComposeSummaryText(TierArea tierArea, List<Entity> personsFor, List<Entity> personsAgainst, List<Entity> personsNotParticipating)
        {
            /*              
                Vote against (-10 Happiness): Lehner, Yeboah
                Vote for: Koppel, Pedersen, Larsen
             *  Not present:
             */

            StringBuilder text = new StringBuilder();
            Common.AppendLine(text);
            Common.AppendDivider(text);
            Common.AppendLine(text, "MINUTES");
            Common.AppendLine(text);

            Common.AppendLine(text, "Adopting " + tierArea.ToString().ToUpper(Config.Culture));

            Common.Append(text, "Vote for: ");
            Common.AppendLine(text, Common.ListToCommaSeparatedString(personsFor, e => SubstituteValue.FormatAllegianceMember(e)));
            Common.Append(text, "Vote against: ");
            string against = Common.ListToCommaSeparatedString(personsAgainst, e => SubstituteValue.FormatAllegianceMember(e));
            Common.AppendLine(text, against);
            Common.Append(text, "Not present: ");
            Common.AppendLine(text, Common.ListToCommaSeparatedString(personsNotParticipating, e => SubstituteValue.FormatAllegianceMember(e)));
            Common.AppendLine(text);
            Common.Append(text, "The following members now have ");
            Statistic.AppendRatingsTypeToStringAndIcon(text, tierArea.Area);
            Common.Append(text, " principles raised to ");
            Common.AppendPercentage(text, tierArea.TierType.GetTierEdgeBelow(), true, null);
            Common.AppendLine(text, ": ");
            Common.AppendLine(text, against);

            return text.ToString();
        }


        private string ComposeMainText(TierArea area, List<Entity> personsFor, List<Entity> personsAgainst)
        {
            /*
             * 
             * GROUP VOTE
                PERSON FOR: bla bla bla
                PERSON AGAINST: bla bla
             * 
                Proposal adopted: Advance to FOOD BASIC tier. FOOD Principles raised to minimum 16.
              
             * 
           // there wil probably be 6 different texts + minutes
            
            // this text is for 2 unhappy, one content:
            
             -representative of MOST Unhappy (Sec): Thanks for taking time out of your day..I want to talk about the security situation. The way it's handled, I don't think we're safe here.
                -Content person/unhappy with something else: I don't see the problem. What do you want done?
                -representative of MOST Unhappy (Sec): I want us to beef up security. How we do it? Stock more and better weapons, take fewer risks...There's a number of ways we can protect our colony better. Main thing is that we take action now.
                -Content person/unhappy with something else: This is all a question of priorities...
                -representative of MOST Unhappy (Sec) Exactly. And if we value our lives, we need better protection from wild animals. So I hope you're with me. If not...well, I might leave for [location name] and start over.
                -Content person/unhappy with something else: Alright. This has been noted. Anything else?

         */

            /*  int maxUnhappy = 0;
              foreach (var item in PersonsFor)
              {
                  if (item.Value.Count > maxUnhappy)
                  {
                      maxUnhappy = item.Value.Count;                   
                  }
              }*/

            if (personsFor.Count + personsAgainst.Count == 1)
            {
                return null;
            }

           
            Entity personForRepresentative = Common.GetRandomListMember(personsFor, The.Sim.GameplayRandomGenerator);
           
           // List<Entity> personsAgainst = new List<Entity>(MeetingParticipants);

           // personsAgainst.RemoveAll(p => PersonsFor[mainIssue].Contains(p));
          
            Entity personAgainstRepresentative = null;
           
            if (personsAgainst.Count > 0)
            {              
                personAgainstRepresentative = Common.GetRandomListMember(personsAgainst, The.Sim.GameplayRandomGenerator);               
            }
           

            string text = "";

            PropertyResult? contentText = null;
          
            if (personForRepresentative != null)
            {              
                
                // take the most specific, default to any one filled
                if (personAgainstRepresentative != null)
                {
                    GetDialogTextProperties(area, out contentText);

                }
                else
                {
                    GetDialogTextPropertiesAllAgree(area, out contentText);
                }

                if (contentText != null)
                {
                    text = contentText.Value.StringResult;

                    text = text.Replace("#FOR", SubstituteValue.FormatAllegianceMember(personForRepresentative)); // unhappyRepresentative.Name);

                    if (personAgainstRepresentative != null)
                    {
                        text = text.Replace("#AGAINST", SubstituteValue.FormatAllegianceMember(personAgainstRepresentative)); // unhappyRepresentative.Name);
                    }
                   
                }
            }     
        

            return text;
        }

        private void GetDialogTextProperties(TierArea tierArea, out PropertyResult? content)
        {
            content = The.Sim.PlaySite.GetPropertyValue(tierArea.KeyName + "PolicyAdopted", null);

        }

        private void GetDialogTextProperties2People(RatingTypes mainIssue, out PropertyResult? content)
        {
            content = The.Sim.PlaySite.GetPropertyValue("meeting2Security", null);
                    

        }

        private void GetDialogTextProperties3OrMore(TierArea tierArea, RatingTypes mainIssue, out PropertyResult? content)
        {
            content = null;
            
            switch (mainIssue)
            {
                case RatingTypes.Security:
                    content = The.Sim.PlaySite.GetPropertyValue("meeting3Security", null);
                    
                    break;

                case RatingTypes.Comfort:
                    content = The.Sim.PlaySite.GetPropertyValue("meeting3Comfort", null);
                    
                    break;

                case RatingTypes.Food:
                    content = The.Sim.PlaySite.GetPropertyValue("meeting3Food", null);
                   
                    break;
            }

        }


        private void GetDialogTextPropertiesAllAgree(TierArea tierArea, /*RatingTypes mainIssue,*/ out PropertyResult? content)
        {
            content = null;

            content = The.Sim.PlaySite.GetPropertyValue(tierArea.KeyName + "PolicyAdoptedAllAgree", null);

            /*
            switch (mainIssue)
            {
                case RatingTypes.Security:
                    content = The.Sim.PlaySite.GetPropertyValue("meetingSecurityAllUnhappy", null);

                    break;

                case RatingTypes.Comfort:
                    content = The.Sim.PlaySite.GetPropertyValue("meetingComfortAllUnhappy", null);

                    break;

                case RatingTypes.Food:
                    content = The.Sim.PlaySite.GetPropertyValue("meetingFoodAllUnhappy", null);

                    break;
            }*/

        }

        /*
        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {            
            sn.Ignore(this.meetingEvent);
          
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

        }

        #endregion*/
    }
}

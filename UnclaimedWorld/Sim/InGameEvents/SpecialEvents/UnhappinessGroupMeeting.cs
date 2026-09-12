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

namespace UWGame.SimSide.InGameEvents.SpecialEvents
{
    /// <summary>
    /// a fixed polled event that can be customized via scenario properties
    /// </summary>
    public class UnhappinessGroupMeetingEvent: ISnapshot
    {

        /*
           // there wil probably be 6 different texts + minutes
            
            // this text is for 2 unhappy, one content:
            
             -representative of MOST Unhappy (Sec): Thanks for taking time out of your day..I want to talk about the security situation. The way it's handled, I don't think we're safe here.
                -Content person/unhappy with something else: I don't see the problem. What do you want done?
                -representative of MOST Unhappy (Sec): I want us to beef up security. How we do it? Stock more and better weapons, take fewer risks...There's a number of ways we can protect our colony better. Main thing is that we take action now.
                -Content person/unhappy with something else: This is all a question of priorities...
                -representative of MOST Unhappy (Sec) Exactly. And if we value our lives, we need better protection from wild animals. So I hope you're with me. If not...well, I might leave for [location name] and start over.
                -Content person/unhappy with something else: Alright. This has been noted. Anything else?

                MINUTES
                Complains about SECURITY conditions: Conlan, Yeboah
                Thinking of leaving for this reason: Conlan

                Wants improvements to FOOD conditions: Kahn
                Thinking of leaving for this reason: Kahn

                Complains about COMFORT conditions: None
             
         */

        private EventActionDialog meetingEvent;
        private AgentCondition canParticipateInMeeting;
       
        /// <summary>
        /// don't pester the player too much, also make sure all expeditions have meetings
        /// </summary>
        private double? totalTimepointInSecondsOfLastMeeting;

        Dictionary<ExpeditionID, double> timepointInSecondsOfLastMeeting;

        /// <summary>
        /// only unhappiness over the limit counts!
        /// those who are not unhappy are content...
        /// </summary>
        Dictionary<RatingTypes, List<Entity>> UnhappyPersons = new Dictionary<RatingTypes, List<Entity>>();
      //  Dictionary<RatingTypes, List<Entity>> ContentPersons = new Dictionary<RatingTypes, List<Entity>>();

        List<Entity> MeetingParticipants = new List<Entity>();

        Regulator updateRegulator;

       
        public UnhappinessGroupMeetingEvent()
        {

            meetingEvent = new EventActionDialog()
            {               
            };

            canParticipateInMeeting = GetMeetingParticipantConditions();

            if (!Snapshotter.IsSnapshotting)
            {
                timepointInSecondsOfLastMeeting = new Dictionary<ExpeditionID, double>();

                //totalTimepointInSecondsOfLastMeeting = GameData.Instance.GUIConstants.DefaultTimeBeforeInGameSecondsBeforeGroupMeeting;

                CreateRegulators();
            }   
        }

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

        }


        void CreateRegulators()
        {
            updateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1f / 10f, "UnhappinessGroupMeetingEvent");
        }

        public void Update() //GameTime gameTime)
        {
            Expedition expedition;
            if (IsTimeForGroupMeeting(out expedition))
            {
                ShowGroupMeeting(expedition);
            }
        }


        private bool IsTimeForGroupMeeting(out Expedition expeditionWithIssues)
        {
            if (updateRegulator.IsReady())
            {
                double timeInGameSecondsBeforeFirstGroupMeeting = GameData.Instance.GUIConstants.DefaultTimeInGameSecondsBeforeGroupMeeting;

                if (totalTimepointInSecondsOfLastMeeting == null)
                {
                    PropertyResult? timeInGameSecondsBeforeGroupMeeting = The.Sim.PlaySite.GetPropertyValue("timeInGameSecondsBeforeGroupMeeting", null);
                    if (timeInGameSecondsBeforeGroupMeeting != null && timeInGameSecondsBeforeGroupMeeting.Value.NumberResult.HasValue)
                    {
                        timeInGameSecondsBeforeFirstGroupMeeting = timeInGameSecondsBeforeGroupMeeting.Value.NumberResult.Value;
                    }                   
                }

                if (The.Sim.TimepointReached(this.totalTimepointInSecondsOfLastMeeting, timeInGameSecondsBeforeFirstGroupMeeting, GameData.Instance.GUIConstants.MinimumTimeInInGameSecondsBetweenGroupMeetings))
                {
                    // can be turned off dynamically:
                    PropertyResult? groupMeetingsEnabled = The.Sim.PlaySite.GetPropertyValue("enableGroupMeetings", null);

                    if (groupMeetingsEnabled != null && groupMeetingsEnabled.Value.BoolResult == true)
                    {
                        expeditionWithIssues = GetExpeditionWithIssues();

                        if (expeditionWithIssues != null)
                        {
                            return true;
                        }
                    }
                }
            }

            expeditionWithIssues = null;
            return false;
        }

        /// <summary>
        /// meetings by expedition..
        /// expeditions don't (yet?) have group statistics.
        /// </summary>
        /// <returns></returns>
        private Expedition GetExpeditionWithIssues()
        {
            
            List<Tuple<Expedition, double>> expeditions = new List<Tuple<Expedition,double>>();

            foreach (var expedition in The.Sim.PlaySite.PlayerAllegiance.Expeditions)
            {
                double timepoint;
                if (timepointInSecondsOfLastMeeting.TryGetValue(expedition.ID, out timepoint))
                {
                    expeditions.Add(new Tuple<Expedition, double>(expedition, timepoint));
                }
                else
                {
                    expeditions.Add(new Tuple<Expedition, double>(expedition, double.MaxValue));
                }               
            }

            if (timepointInSecondsOfLastMeeting.Count > The.Sim.PlaySite.PlayerAllegiance.Expeditions.Count)
            {
                // clean up
                Dictionary<ExpeditionID, double> newTimepointInSecondsOfLastMeeting = new Dictionary<ExpeditionID,double>();

                foreach (var item in timepointInSecondsOfLastMeeting)
                {
                    if (The.Sim.PlaySite.PlayerAllegiance.Expeditions.Exists(e => e.ID == item.Key))
                    {
                        newTimepointInSecondsOfLastMeeting.Add(item.Key, item.Value);
                    }
                }

                timepointInSecondsOfLastMeeting = newTimepointInSecondsOfLastMeeting;
            }

            
            var ordered = expeditions.OrderByDescending(t => t.Item2);

            foreach (var item in ordered)
            {
                if (ExpeditionHasIssues(item.Item1))
                {
                    return item.Item1;
                }
            }
            // TODO: set the timepoints

            return null;          
        }

        /// <summary>
        /// use the overall Happiness as trigger
        /// </summary>
        /// <param name="expedition"></param>
        /// <returns></returns>
        private bool ExpeditionHasIssues(Expedition expedition)
        {
            int unhappyMembers = 0;
            int total = 0;

            List<EntityID> obsolete = null;
            foreach (var item in expedition.IndependentMembers)
            {
                Entity entity = Entity.FindByID(item);
                if (entity != null 
                    && entity.Intelligence.HasHappiness()
                    && entity.PersonEntity.Personality.CanComplainProperty())
                {
                    //(float)PersonEntity.Personality.Happiness; 
                    
                    total++;

                    // one member enough to trigger..?
                    if (entity.PersonEntity.Personality.Happiness < GameData.Instance.AIConstants.Ratings.HappinessLimitForGroupMeeting
                        && canParticipateInMeeting.IsFulfilled(entity))
                    {
                        unhappyMembers++;
                        //return true;                        
                    }
                }
                else
                {
                    Common.AddToList(ref obsolete, item);
                }
            }

            if (obsolete != null)
            {
                foreach (var item in obsolete)
                {
                    expedition.RemoveMemberID(item);
                }
            }
                        
            if ((float)unhappyMembers / (float)total >= GameData.Instance.AIConstants.Ratings.UnhappyExpeditionMembersPercentageForGroupMeeting // need a certain % to be unhappy
                /*&& unhappyMembers < total*/) // content members must still be available in order to have the meeting
            {
                return true;
            }

            return false;
        }



        private void ShowGroupMeeting(Expedition expedition)
        {
            GroupMembersByIssues(expedition);
                       
            string displayText = ComposeMainText();

            // only show the dialog if there was any text result:

            if (!string.IsNullOrEmpty(displayText))
            {
                string minutesText = ComposeMinutesText();

                displayText += minutesText;

                meetingEvent.Heading = "GROUP MEETING";
                meetingEvent.DisplayText = new DynamicText();
                meetingEvent.DisplayText.Text = displayText;
                meetingEvent.DisplayImage = "GroupMeeting"; 
               

                string failReason = null;
                meetingEvent.Execute(null, ref failReason);
            }


            totalTimepointInSecondsOfLastMeeting = The.Sim.TotalUnPausedGameTimeInSeconds;
            timepointInSecondsOfLastMeeting[expedition.ID] = The.Sim.TotalUnPausedGameTimeInSeconds;

            UnhappyPersons.Clear();
            MeetingParticipants.Clear();
        }



        private void GroupMembersByIssues(Expedition expedition)
        {
            var ratingTypes = Enum.GetValues(typeof(RatingTypes));

            foreach (var item in expedition.IndependentMembers)
            {
                Entity entity = Entity.FindByID(item);
                if (entity != null 
                    && entity.Intelligence.HasHappiness() 
                    && entity.PersonEntity.Personality.CanComplainProperty()
                    && canParticipateInMeeting.IsFulfilled(entity))
                {
                    MeetingParticipants.Add(entity);

                    foreach (var rating in ratingTypes)
	                {
                        float happiness = entity.PersonEntity.Personality.ComputeHappinessComponent((RatingTypes)rating);
                        //entity.Intelligence.Statistics.GetRating())
		 
                        if (happiness < GameData.Instance.AIConstants.Ratings.HappinessLimitForGroupMeeting)
                        {
                            Common.AddToMultiList(UnhappyPersons, (RatingTypes)rating, entity);
                        }
	                }


                }
            }


            //The.Sim.PlaySite.PlayerAllegiance 

        }

        private string ComposeMinutesText()
        {
            /*
              MINUTES
                Complains about SECURITY conditions: Conlan, Yeboah
                Thinking of leaving for this reason: Conlan

                Wants improvements to FOOD conditions: Kahn
                Thinking of leaving for this reason: Kahn

                Complains about COMFORT conditions: None
             */

            StringBuilder minutes = new StringBuilder();
            Common.AppendLine(minutes);
            Common.AppendDivider(minutes);
            Common.AppendLine(minutes, "MINUTES");
            Common.AppendLine(minutes);

            string complainsTerm = "Complains about ";
            foreach (var item in UnhappyPersons)
            {
                if (item.Value.Count > 0)
                {
                    minutes.Append(complainsTerm);
                   // minutes.Append(Statistic.RatingsTypeToString(item.Key).ToUpper(Config.Culture));
                    Statistic.AppendRatingsTypeToStringAndIcon(minutes, item.Key);

                    minutes.Append(" conditions: ");

                    string delim = "";
                    foreach (var person in item.Value)
                    {
                        minutes.Append(delim);
                        minutes.Append(SubstituteValue.FormatAllegianceMember(person)); 
                      //  minutes.Append(person.GetDisplayName()); 

                        delim = ", ";
                    } 
                    
                   
                    delim = "";
                    bool hasAddedEmigrants = false;
                    foreach (var person in item.Value)
                    {
                        if (person.Intelligence.EmigrateDecider != null &&
                            person.Intelligence.EmigrateDecider.CanEmigrateToAnyTarget() && //CanEmigrate() &&
                            person.Intelligence.EmigrateDecider.MigrationRisk > 0f)
                        {
                            if (!hasAddedEmigrants)
                            {
                                Common.AppendLine(minutes);

                                minutes.Append("Thinking of leaving for this reason: ");
                            }                          

                            Common.AppendLine(minutes);

                           // minutes.Append(delim);
                            minutes.Append(SubstituteValue.FormatAllegianceMember(person));
                            Common.Append(minutes, " ");
                            Common.AppendPercentage(minutes, person.Intelligence.EmigrateDecider.MigrationRisk, false, null);

                          //  delim = ", ";
                            hasAddedEmigrants = true;

                            /*
                            if (!hasAddedEmigrants)
                            {
                                Common.AppendLine(minutes);

                                minutes.Append("Thinking of leaving for this reason: ");
                            }

                            minutes.Append(delim);
                            minutes.Append(SubstituteValue.FormatAllegianceMember(person));
                            Common.AppendPercentage(minutes, person.Intelligence.EmigrateDecider.MigrationRisk, false, null);
                           
                            delim = ", ";
                            hasAddedEmigrants = true;*/
                        }
                    }

                   
                    Common.AppendLine(minutes);
                    Common.AppendLine(minutes);

                    complainsTerm = "Wants improvements to ";
                }
            }

            return minutes.ToString();
        }


        private string ComposeMainText()
        {
            /*
              
           // there wil probably be 6 different texts + minutes
            
            // this text is for 2 unhappy, one content:
            
             -representative of MOST Unhappy (Sec): Thanks for taking time out of your day..I want to talk about the security situation. The way it's handled, I don't think we're safe here.
                -Content person/unhappy with something else: I don't see the problem. What do you want done?
                -representative of MOST Unhappy (Sec): I want us to beef up security. How we do it? Stock more and better weapons, take fewer risks...There's a number of ways we can protect our colony better. Main thing is that we take action now.
                -Content person/unhappy with something else: This is all a question of priorities...
                -representative of MOST Unhappy (Sec) Exactly. And if we value our lives, we need better protection from wild animals. So I hope you're with me. If not...well, I might leave for [location name] and start over.
                -Content person/unhappy with something else: Alright. This has been noted. Anything else?

               
         */
            RatingTypes mainIssue = RatingTypes.Security;
            int maxUnhappy = 0;
            foreach (var item in UnhappyPersons)
            {
                if (item.Value.Count > maxUnhappy)
                {
                    maxUnhappy = item.Value.Count;
                    mainIssue = item.Key;
                }
            }

            string emigrateTo = "";
            Entity unhappyRepresentative = Common.GetRandomListMember(UnhappyPersons[mainIssue], The.Sim.GameplayRandomGenerator);
            if (unhappyRepresentative.Intelligence.EmigrateDecider.CanEmigrateToAnyTarget()) //CanEmigrate())
            {
                if (unhappyRepresentative.Intelligence.EmigrateDecider.PreferredMigrationTarget.HasValue 
                    && unhappyRepresentative.Intelligence.EmigrateDecider.MigrationRisk > 0f)
                {
                    Allegiance target = LookUp<Allegiance, AllegianceID>.FindByID(unhappyRepresentative.Intelligence.EmigrateDecider.PreferredMigrationTarget.Value);
                    if (target != null)
                    {
                        emigrateTo = target.Site.Name;
                    }
                }

            }


            List<Entity> content = new List<Entity>(MeetingParticipants);

            content.RemoveAll(p => UnhappyPersons[mainIssue].Contains(p));
            // could still be unhappy about other issues

            Entity contentRepresentative = null;
            PropertyResult? emigrateThreat;

            if (content.Count > 0)
            {              
                contentRepresentative = Common.GetRandomListMember(content, The.Sim.GameplayRandomGenerator);
                emigrateThreat = The.Sim.PlaySite.GetPropertyValue("meetingEmigrateThreat", null);
            }
            else
            {
                // all unhappy / All agree                 
                emigrateThreat = The.Sim.PlaySite.GetPropertyValue("meetingEmigrateThreatAllUnhappy", null);
            }

            string text = "";

            PropertyResult? contentText = null;
          
            if (unhappyRepresentative != null)
            {
                // take the most specific, default to any one filled
                if (contentRepresentative != null)
                {
                    if (MeetingParticipants.Count == 2)
                    {
                        GetDialogTextProperties2People(mainIssue, out contentText);
                    }

                    if (contentText == null)
                    {
                        GetDialogTextProperties3OrMore(mainIssue, out contentText);
                    }
                }
                else
                {
                    // all unhappy / All agree                 
                    GetDialogTextPropertiesAllAgree(mainIssue, out contentText);
                }

                if (contentText != null)
                {
                    text = contentText.Value.StringResult;

                    text = text.Replace("#UNHAPPY", SubstituteValue.FormatAllegianceMember(unhappyRepresentative)); // unhappyRepresentative.Name);

                    if (contentRepresentative != null)
                    {
                        text = text.Replace("#CONTENT", SubstituteValue.FormatAllegianceMember(contentRepresentative));
                    }

                    if (!string.IsNullOrEmpty(emigrateTo) && emigrateThreat.HasValue)
                    {
                        string threat = emigrateThreat.Value.StringResult;
                        threat = threat.Replace("#EMIGRATETO", emigrateTo);

                        text = text.Replace("#EMIGRATETHREAT", threat);
                    }
                    else
                    {
                        text = text.Replace("#EMIGRATETHREAT", "");
                    }

                }
            }     
        
           

          /*  if (The.Sim.PlaySite.PlayerAllegiance.Members.Count > 1)
            {
                if (PlayerEntityDeaths.Count == 1)
                {

                    mainText = GetBurialText(singleDeathMultipleSurvivors, singleDeathSingleSurvivor);
                   
                }
                else
                {
                    // string names = Common.ListToCommaSeparatedString(PlayerEntityDeaths, e => e.EntityName);

                    mainText = GetBurialText(multipleDeathsMultipleSurvivors, multipleDeathsSingleSurvivor);
                  

                }
            }
            else
            {
                // one guy left...
                if (PlayerEntityDeaths.Count == 1)
                {

                    mainText = GetBurialText(singleDeathSingleSurvivor, singleDeathMultipleSurvivors);
                   
                }
                else
                {
                    //string names = Common.ListToCommaSeparatedString(PlayerEntityDeaths, e => e.EntityName);

                    mainText = GetBurialText(multipleDeathsSingleSurvivor, multipleDeathsMultipleSurvivors);
                   
                }
            }


            string headerText = "";
            if (burialTextStart != null && !string.IsNullOrEmpty(burialTextStart.Value.StringResult))
            {
                if (includeDateInBurialHeader != null && includeDateInBurialHeader.Value.BoolResult == true)
                {
                    // include the date also:
                    headerText = The.Sim.DateAndTime.CurrentTimeDateYear.GetDateForJournal() + " \n \n";
                }

                // append a common starting text ("Date: 03-10 2238 \nLocation: 4° 12' 22...") if it was defined.
                headerText += burialTextStart.Value.StringResult;
            }

            return SubstituteBurialText(headerText + mainText);

            */

           // mainText = SubstituteMainText(mainText);

            return text;
        }

        private void GetDialogTextProperties2People(RatingTypes mainIssue, out PropertyResult? content)
        {
            content = null;

            switch (mainIssue)
            {
                case RatingTypes.Security:
                    content = The.Sim.PlaySite.GetPropertyValue("meeting2Security", null);
                    
                    break;

                case RatingTypes.Comfort:
                    content = The.Sim.PlaySite.GetPropertyValue("meeting2Comfort", null);
                   
                    break;

                case RatingTypes.Food:
                    content = The.Sim.PlaySite.GetPropertyValue("meeting2Food", null);
                    
                    break;
            }

        }

        private void GetDialogTextProperties3OrMore(RatingTypes mainIssue, out PropertyResult? content)
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


        private void GetDialogTextPropertiesAllAgree(RatingTypes mainIssue, out PropertyResult? content)
        {
            content = null;

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
            }

        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.timepointInSecondsOfLastMeeting = sn.DoDictionary(timepointInSecondsOfLastMeeting);
            this.totalTimepointInSecondsOfLastMeeting = sn.DoDoubleNullable(totalTimepointInSecondsOfLastMeeting);


            sn.Ignore(this.meetingEvent);
            sn.Ignore(MeetingParticipants);
            sn.Ignore(this.canParticipateInMeeting);
            sn.Ignore(UnhappyPersons);

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


            CreateRegulators();
        }

        #endregion
    }
}

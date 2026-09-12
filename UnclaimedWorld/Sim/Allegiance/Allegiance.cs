using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.Jobs;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Allegiances.Statistics;

using UWGame.SimSide.AI.Planners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Policies;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.AI.Goals;
using UWGame.ClientSide.PropertyPresentation;
using System.Diagnostics;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Overland.Templates;
using UWGame.Steam;
namespace UWGame.SimSide.Allegiances
{

    /// <summary>
    /// These constants correspond to events during the allegiance's lifecycle. They can be used for firing scripted events.
    /// since these represent points in the program it is OK to make them a compiled enum type.
    /// 
    /// Not sure if it is a good idea to make player-specifc events... there needs a condition on events anyways...
    /// </summary>
    public enum AllegianceEvents
    {
        Created, Destroyed, TransportArrivedOnMap, TransportHasUnloaded, TransportDepartingMap, CargoDeliveredToPlayer, TransportToPlayerAborted //, ImmigrantArrives
    }


    public enum AllegianceID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// needed...?
    /// </summary>
    public enum AllegianceType { Player, Other }

    /// <summary>
    /// a container for the cooperating entities at a Site. The player has one, and there may be independents as well...   
    /// 
    /// 
    /// </summary>
    [DebuggerDisplay("{KeyName}")]
    public class Allegiance : IHasExposedProperties, ICanIterateEntities, ILookUp<Allegiance, AllegianceID>, ISnapshot, IHasEntityGroup, ICommunicates
    {
        /// <summary>
        /// Required!
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        public string KeyName
        {
            get;
            private set;
        }

        public Color DebugColor;

        private Site site;
       
        public Site Site
        {
            get { return site; }
            set 
            {
               
                site = value; 
            }
        }
      /*  public Site Site 
        { 
            get; 
            set; 
        }*/

        public GeodeticCoordinate? Coords
        {
            get
            {
                if (Site != null)
                {
                    return Site.Coords;
                }

                return null;
            }
        }

        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();

        private Dictionary<string, PropertyResult> customFields;

        private SiteID snapshotSiteID;

        public AllegianceType AllegianceType;

        /// <summary>
        /// contains intelligent agents (sensors, terminals too), though probably should not contain animals
        /// </summary>      
        public HashSet<Entity> Members = new HashSet<Entity>();
        public List<Entity> MembersList = new List<Entity>(); // for iterating
        private List<EntityID> snapshotMembers = new List<EntityID>();

        /// <summary>
        /// Same as in Expedition: filtered members, by independent status - these are the ones that can leave and vote
        /// </summary>
        public List<EntityID> IndependentMembers = new List<EntityID>();
      


        /// <summary>
        /// convenience list, mostly used in UI and scripting
        /// </summary>
        public List<Entity> Persons = new List<Entity>(); 
       
       
        public GroupStatistics Statistics;


        public bool PermitsImmigration;

        public List<Expedition> Expeditions = new List<Expedition>();
        private List<ExpeditionID> snapshotExpeditions = new List<ExpeditionID>();

        /// <summary>
        /// also per expedition..
        /// </summary>
        public AllegiancePolicy Policy = new AllegiancePolicy();


        /// <summary>
        /// these allegiances can see/unsee terminals and their offered content, as well as all allegiance members
        /// Only sentients here...
        /// </summary>
        public HashSet<AllegianceID> AllegiancesWeAreInContactWith = new HashSet<AllegianceID>();

        /// <summary>
        /// we can communicate for short distances beyond the site...
        /// </summary>
    //    public List<CommunicatorType> IntrinsicCommunicators;
       // public List<Communicator> IntrinsicCommunicators;


        /// <summary>
        /// owned transports en route.
        /// 
        /// each mission has a Job as well - in EntityGroup
        /// </summary>
        public List<Mission> Missions;
        List<MissionID> snapshotMissions;

        private decimal? tradeCredits = 0;
        public decimal? TradeCredits
        {
            get { return tradeCredits; }
            set { tradeCredits = value; }
        }

        // organize with regions and methods to make it apparent which code is in use in playsite and which in othersite allegiances
        #region Othersite fields

        public OtherSiteAllegianceManager OtherSiteAllegianceManager;




        #endregion


        #region Playsite fields

        /// <summary>
        /// NEW: use in othersite also.
        /// </summary>
        public SharedKnowledge SharedKnowledge;

        //   public ThreatCategory ThreatCategory;
        //  public StrengthRating StrengthRating;

        /// <summary>
        /// the species that represents this allegiance - will there ever be multi-species allegiances? I think so...
        /// robots will be in the allegiance...
        /// </summary>
        public EntityType RepresentativeEntityType;


        public FoodExtraction FoodExtraction;


        public ThreatJobManager ThreatAndCombatJobManager;
        CyclableID? snapshotThreatManager;



        /// <summary>
        /// is a null reference for the animal allegiances... Keep this???
        /// </summary>
        public HumanActivities HumanActivities;

        /// <summary>
        /// On playsite, this is never null. entities and allegiances in the same threat group will not see each other as threats on the threat map...
        /// </summary>
        public ThreatGroup ThreatGroup;
        private ThreatGroupID? snapshotThreatGroupID;

        #endregion

        //TODO: make a historical log of all members living and dead with their names and statistics

        public Allegiance()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public Allegiance(AllegianceType allegianceType, EntityType representativeEntityType, string key = null, string name = null, bool computeAuxiliaryMaps = true, Site site = null)
        {
            AddToLookup();
            ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).AddToLookup();
            ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).AddToLookup();

            Site = site ?? The.Sim.PlaySite;

            if (key == null)
            {
                this.KeyName = representativeEntityType.KeyName + "Allegiance#" + ID.ToString();
            }
            else
            {
                this.KeyName = key;
            }

          
            this.Name = name;
            this.AllegianceType = allegianceType;
            this.RepresentativeEntityType = representativeEntityType;


            Site.Allegiances.Add(this);

            if (AllegianceType == global::UWGame.SimSide.Allegiances.AllegianceType.Player)
            {
                The.InGameUI.UIAllegiance = this;
                
            }
            
            ulong colorId = (ulong)id + 1;
            DebugColor = new Color((byte)((colorId * 100) % 255), (byte)((colorId * 23) % 255), (byte)((colorId * 7) % 255));

            HumanActivities = new HumanActivities();

            SharedKnowledge = new AI.SharedKnowledge(this);

            if (Site.IsPlaySite) // site == null)
            {
                InitPlaySite(The.Sim.IsInNormalGameLoop); //computeAuxiliaryMaps);
            }
            else
            {
                //Manager = new OtherSiteAllegianceManager(this);
            }



            OtherSiteAllegianceManager = new OtherSiteAllegianceManager(this);

            Expedition expedition = GetFirstExpedition();

            // for every allegiance, not just on play site.
            Statistics = new GroupStatistics(this /*canIterateEntitiesID*/, representativeEntityType);



            // play site only? no, othersite allegiances may do spawns too...
            if (RepresentativeEntityType.PolledEvents != null)
            {
                List<PolledEventType> list;
                if (RepresentativeEntityType.PolledEvents.TryGetValue(Scope.Allegiance, out list))
                {
                    foreach (var item in list)
                    {
                        if (Site.IsPlaySite || item.PlaySiteOnly == false)
                        {
                            PolledEvent polledEvent = Site.EventManager.AddPolledEvent(item.KeyName, null, null, this.ID);
                        }
                    }
                }
            }
        }

        static Allegiance()
        {
            exposedPropertyValueFunctions.Add("keyName", GetKeyName);
            exposedPropertyValueFunctions.Add("name", GetName);

            exposedPropertyValueFunctions.Add("comfortRating", GetComfortRating);
            exposedPropertyValueFunctions.Add("foodRating", GetFoodRating);
            exposedPropertyValueFunctions.Add("securityRating", GetSecurityRating);

        }

      
        public int? GetMaxPopulationMembers()
        {
            int? max = null;

            if (Expeditions != null)
            {
                foreach (var item in Expeditions)
                {
                    if (item.Population != null)
                    {
                        if (max == null)
                        {
                            max = 0;
                        }

                        max += item.Population.MaxMembers;
                    }
                }
            }

            return max;
        }

        # region Code used only on play site

        /// <summary>
        /// Called as update when allegiance is on the current play site.
        /// </summary>
        public void UpdatePlaySite(GameTime gameTime)
        {

#if DEBUG || PROFILE
            if (MembersHaveAIDisabled())
            {
                return;
            }
#endif

            if (AllegianceType == AllegianceType.Player)
            {
                if (Statistics != null)
                {
                    Statistics.Update(gameTime);

                    Statistics.CheckAchievements();
                }

            }
           

            ThreatAndCombatJobManager.Update(gameTime);

            if (HumanActivities != null)
            {
                HumanActivities.Update(gameTime);
            }

            SharedKnowledge.Update(gameTime);
        }


        void FoodExtraction_FoodProcessesChanged()
        {
            // make sure that the list of known food items gets updated:          
            SharedKnowledge.SetFoodDirty();
        }

        #endregion

        # region Code used only for other-site allegiances

        /// <summary>
        /// Called as update when allegiance is NOT on the current play site.
        /// </summary>
        public void UpdateOffPlaySite(GameTime gameTime)
        {
            OtherSiteAllegianceManager.Update(gameTime);
        }

        #endregion


      
        /// <summary>
        /// call this when a new communicator goes up - the oher allegiance must now get knowledge about all our terminals.
        /// buy/trade terminals are never in FOW
        /// other terminals might be... so this would require copying memory facts...
        /// </summary>
        /// <param name="otherAllegiance"></param>
        public void GainContact(Allegiance otherAllegiance)
        {
            if (otherAllegiance.RepresentativeEntityType.IntelligenceType.CanTradeAndCommunicate == true
                && !AllegiancesWeAreInContactWith.Contains(otherAllegiance.ID))
            {
                this.AllegiancesWeAreInContactWith.Add(otherAllegiance.ID);

                LetOtherAllegianceGetKnowledgeAboutTerminals(otherAllegiance);
                LetOtherAllegianceGetKnowledgeAboutMembers(otherAllegiance);
            }
        }

        private void LetOtherAllegianceGetKnowledgeAboutMembers(Allegiance otherAllegiance)
        {
            foreach (var item in Members)
            {
                //otherAllegiance.SharedKnowledge.SeeDetectable(item);
                otherAllegiance.SharedKnowledge.SeeDetectableIfRelevant(item);
            }
        }

        public void LogProductionStatistics(IKnownEntityData entityData, IKnownProcess processData)
        {
            Statistics.AddProductionEvent(entityData.EntityType, ProductionStatistics.StatTypes.Produced, 1);

            if (processData != null)
            {
                Statistics.AddProductivityEvent(entityData.EntityType, processData);
            }


            // log nutrients too:
            if (IsEatable(entityData.EntityType) && entityData.EntityType.ItemType != null && entityData.EntityType.ItemType.FoodType != null)
            {
                foreach (var item in entityData.NutrientBulkAmounts) //.Item.Food.NutrientBulkAmounts)
                {
                    Statistics.AddNutrientEvent(item.Key, NutrientStatistics.StatTypes.Produced, item.Value);
                }
            }
        }


        private void LetOtherAllegianceGetKnowledgeAboutTerminals(Allegiance otherAllegiance)
        {
            foreach (var item in Expeditions)
            {
                foreach (var list in item.OwnedEntities.Terminals)
                {
                    foreach (var terminalID in list.Value)
                    {
                        IKnownEntityData entityData;
                        if (!GoalEvaluator.EntityDataResultCausesSkip(SharedKnowledge.GetKnownData(terminalID, out entityData)))
                        {
                            Entity terminal = entityData as Entity;
                            if (terminal != null)
                            {
                                //otherAllegiance.SharedKnowledge.SeeDetectable(terminal);
                                otherAllegiance.SharedKnowledge.SeeDetectableIfRelevant(terminal, doAssert: false);
                            }
                            else
                            {
                                // copy a memory fact? not implemented, but could be useful, also for low tech scenarios where no communication exists, an explorer would return with memory of terminals...
                                // for now a terminal must be seen directly by owner (= not in FOW) in order to trade. we enforce this by including a sensor on the structure...


                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// call this to let other allegiances in contact see the entity, or see it be destroyed.
        /// the second case is different from when we lose contact, because no memory fact gets created.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="seeEntity"></param>
        public void LetOtherAllegiancesSeeEntity(Entity entity, bool seeEntity)
        {

            List<AllegianceID> outdatedAllegiances = null;
            foreach (var allegianceID in AllegiancesWeAreInContactWith)
            {
                Allegiance otherAllegiance = LookUp<Allegiance, AllegianceID>.FindByID(allegianceID);

                if (otherAllegiance != null)
                {

                    if (seeEntity)
                    {
                       // otherAllegiance.SharedKnowledge.SeeDetectable(entity);
                        otherAllegiance.SharedKnowledge.SeeDetectableIfRelevant(entity);
                    }
                    else
                    {
                        otherAllegiance.SharedKnowledge.DeleteMemoryOfEntity(entity.ID, entity.DetectableID, true);
                    }
                    // the terminal becomes unseen (memory fact) when communication is lost, for instance when a communicator is destroyed

                }
                else
                {
                    Common.AddToList(ref outdatedAllegiances, allegianceID);
                }
            }

            // clean up...
            if (outdatedAllegiances != null)
            {
                foreach (var outdated in outdatedAllegiances)
                {
                    AllegiancesWeAreInContactWith.Remove(outdated);
                }
            }

        }

        /// <summary>
        /// call this when a communicator stops working - they should no longer see our terminals or members
        /// </summary>
        /// <param name="otherAllegiance"></param>
        public void LoseContact(Allegiance otherAllegiance)
        {
            this.AllegiancesWeAreInContactWith.Remove(otherAllegiance.ID);

            foreach (var item in Expeditions)
            {
                foreach (var list in item.OwnedEntities.Terminals)
                {
                    foreach (var terminalID in list.Value)
                    {
                        Entity terminal = Entity.FindByID(terminalID);
                        if (terminal != null)
                        {
                            otherAllegiance.SharedKnowledge.UnSeeEntity(terminal); // creates a memory fact!
                        }
                    }
                }
            }

            foreach (var item in Members)
            {
                otherAllegiance.SharedKnowledge.UnSeeEntity(item); // creates a memory fact!
            }

        }

        /// <summary>
        /// no sleepy updates...
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {

            foreach (Expedition expedition in Expeditions)
            {               
                expedition.Update(gameTime);
            }

            if (Missions != null)
            {
                for (int i = Missions.Count - 1; i >= 0; i--)
                {
                    Mission transport = Missions[i];

                    transport.Update(gameTime);
                }
            }


            if (Site.IsPlaySite)
            {
                UpdatePlaySite(gameTime);
            }
            else
            {
                UpdateOffPlaySite(gameTime);
            }

        }




        /*  public void ResetFoodRations()
          {
              foreach (Expedition e in Expeditions)
              {
                  e.ResetFoodRations();
              }
          }*/

        private int? forageAndHuntingRadius = null;
        public int GetForageAndHuntingRadius()
        {
            if (forageAndHuntingRadius.HasValue)
            {
                return forageAndHuntingRadius.Value;
            }
            return RepresentativeEntityType.IntelligenceType.ForageAndHuntingRadius;
        }


        public void AddMission(Mission mission)
        {
            Common.AddToList(ref Missions, mission);

        }

        public void RemoveMission(Mission mission)
        {
            Missions.Remove(mission); // we should be able to do this while iterating in a reverse for loop

        }

        /// <summary>
        /// Initializes necesarry code to convert an allegience from an other site to a playsite allegiance
        /// </summary>
        public void InitPlaySite(bool computeAuxiliaryMaps)
        {

            if (HumanActivities == null)
            {
                HumanActivities = new HumanActivities();
            }

            //SharedKnowledge = new AI.SharedKnowledge(this); 

            FoodExtraction = new Allegiances.FoodExtraction(this, SharedKnowledge.AllKnownEntities.ID);


            // OPTIMIZED: this should happen AFTER the terrain map has been fully drawn.
            if (computeAuxiliaryMaps
                && The.Map != null
                && The.Map.mapTileWidth > 0)
            {
                SharedKnowledge.PlaySiteKnowledge.InitAuxiliaryMaps();
            }

            ThreatAndCombatJobManager = new ThreatJobManager(this);


            ThreatGroup = ThreatGroup.GetThreatGroup(Name);

        }



        /// <summary>
        /// Creates an Allegiance from an AllegianceData
        /// </summary>
        public static Allegiance CreateFromAllegianceData(AllegianceData allegianceData, Site site = null, float? sizeFactor = null, 
            string allegianceKeyName = null, string expeditionKeyName = null)
        {
            EntityType representativeEntityType = GameData.Instance.AllEntityTypes[allegianceData.EntityType];

            if (site == null)
            {
                site = The.Sim.World.AllSites[allegianceData.Site];
            }

            Allegiance allegiance = new Allegiance(allegianceData.AllegianceType,
                                                       representativeEntityType,
                                                       allegianceKeyName ?? /*allegianceData.AllegianceKeyName ??*/ allegianceData.KeyName,
                                                       allegianceData.Name ?? site.Name,
                                                       true, site);

          /*  Allegiance allegiance = new Allegiance(allegianceData.AllegianceType,
                representativeEntityType,
                allegianceData.KeyName, 
                allegianceData.Name ?? site.Name, 
                true, site) 
                {
                    KeyName = allegianceData.KeyName
                };*/


            //TODO: Add this to constructor. - ??
            allegiance.forageAndHuntingRadius = allegianceData.ForageAndHuntingRadius;

            allegiance.PermitsImmigration = allegianceData.PermitsImmigration;

            if (allegiance.AllegianceType != AllegianceType.Player)
            {
                allegiance.Statistics = GroupStatistics.CreateFromStatsData(allegianceData.StatsData, allegiance, representativeEntityType);
            }

            // allegiance.Policy = AllegiancePolicy.CreateFromPolicyData(allegianceData.PolicyData);
            

            if (allegiance.AllegianceType == AllegianceType.Player && site.IsPlaySite) // allegianceData.Site.IsPlaySite)
            {
                The.Sim.PlaySite.PlayerAllegiance = allegiance;              
            }

            // NEW: fill from template
            if (allegianceData.AllegianceTemplates != null)
            {
                int index;
                StringChance trait = Common.GetStairStepIndex(allegianceData.AllegianceTemplates, out index, The.Sim.GameplayRandomGenerator);

                AllegianceTemplate template = GameData.Instance.AllAllegianceTemplates[trait.String];

                template.FillAllegiance(allegiance, sizeFactor, expeditionKeyName);
            }


            return allegiance;
        }


        /// <summary>
        /// not sure why we want to do this with ordered structures like the sentry...
        /// </summary>
        /// <param name="newMember"></param>
        public void AddMember(Entity newMember)
        {
          
            Members.Add(newMember);
            MembersList.Add(newMember);

            if (Site.IsPlaySite)
            {
                if (SharedKnowledge.PlaySiteKnowledge != null)
                {
                    // use a temp location  if needed:
                    Vector2 loc = (newMember.Location ?? Vector3.Zero).ToVector2();
                    SharedKnowledge.PlaySiteKnowledge.AddOrUpdateKnownEntityLocation(newMember, loc);
                    //SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.AddObject(newMember.EntityID, loc);
                }

                SharedKnowledge.AddMember(newMember);
            }

            if (newMember.Intelligence != null && newMember.Intelligence.IsIndependent())
            {
                IndependentMembers.Add(newMember.ID);
            }

            if (newMember.EntityType.Person != null)
            {            
                Persons.Add(newMember);

                CheckAchievements();
            }

            if (Site.IsPlaySite)
            {
                HandleGroupMembersChanged(FoodExtraction);
            }

            NotifyPopulationStatistics();
                        
            // location has not been set yet... will cause a crash if there are ever 2 or more sentient playsite allegiances.
            LetOtherAllegiancesSeeEntity(newMember, true);

            
        }


        void CheckAchievements()
        {
            if (this.AllegianceType != Allegiances.AllegianceType.Player)
            {
                return;
            }

            if (The.Sim.StartGameParams.GetRGScenario() == Scenarios.StartGameParams.RGScenario.FieldsOfTauCeti)
            {
                StatsAndAchievements ach = The.Sim.Controller.StatsAndAchievements;
         
                if (!ach.IsAchievementUnlocked(AchievementID.relatives))
                {
                    var groups = Persons.GroupBy(e => e.Intelligence.LastName);

                    foreach (var item in groups)
                    {
                        if (item.Count() >= 3)
                        {
                            ach.UnlockAchievement(AchievementID.relatives);
                            break;
                        }
                    }                   

                }
            }

        }

        private void NotifyPopulationStatistics()
        {
            if (RepresentativeEntityType.Person != null)
            {
                // for human allegiances, don't count robots, sensors, animals...
                Statistics.NotifyPopulationChanged(Persons.Count);
            }
            else
            {
                Statistics.NotifyPopulationChanged(MembersList.Count);
            }
        }

        public static void HandleGroupMembersChanged(FoodExtraction foodExtraction)
        {
            foodExtraction.SetIsDirty();
        }

        public void RemoveMember(Entity memberToRemove, bool isDestroyed)
        {
            Members.Remove(memberToRemove);
            MembersList.Remove(memberToRemove);

            if (SharedKnowledge != null
                && SharedKnowledge.PlaySiteKnowledge != null)
            {
                SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.RemoveObject(memberToRemove.EntityID);
            }

            if (memberToRemove.EntityType.Person != null)
            {               
                Persons.Remove(memberToRemove);
            }

            if (memberToRemove.EntityType.IntelligenceType != null)
            {
                IndependentMembers.Remove(memberToRemove.ID);
            }

            if (Site.IsPlaySite)
            {
                HandleGroupMembersChanged(FoodExtraction);

                SharedKnowledge.RemoveMember(memberToRemove);
            }

            NotifyPopulationStatistics();

            if (memberToRemove.Intelligence.IsIndependent())
            {
                Statistics.RecordDeathOrEmigration(isDestroyed);

               
            }

            if (!isDestroyed)
            {
                LetOtherAllegiancesSeeEntity(memberToRemove, false);
            }

        }

        public void GetMembersInTierRange(RatingTypes rating, TierType tier, AgentCondition agentCanVote,
            out List<Entity> membersAboveRange, out List<Entity> membersBelowRange, out List<Entity> membersUnqualified, /*out List<Entity> membersWhoWouldHavePrinciplesRaised,*/ out TierType previousTier)
        {
            membersAboveRange = new List<Entity>();
            membersBelowRange = new List<Entity>();
            membersUnqualified = new List<Entity>();
          //  membersWhoWouldHavePrinciplesRaised = new List<Entity>();

            float upperTierRange = tier.UpperEdge;
            int tierIndex = Array.FindIndex(GameData.Instance.Tiers, t => t == tier);

            float lowerTierEdge;
            TierType.GetTierBelow(tierIndex, out previousTier, out lowerTierEdge);
            if (previousTier == null)
            {
                lowerTierEdge = -1f;
            }


            //List<Entity> list = null;
            foreach (var item in IndependentMembers)
	        {
                Entity entity = Entity.FindByID(item);

                if (entity.PersonEntity != null)
                {
                    if (agentCanVote.IsFulfilled(entity))
                    {
                        float principle = entity.PersonEntity.Personality.Principles[rating];
                        if (principle > lowerTierEdge) // && principle <)
                        {
                            Common.AddToList(ref membersAboveRange, entity);
                        }
                        else
                        {
                            Common.AddToList(ref membersBelowRange, entity);
                        }
                    }
                    else
                    {
                        Common.AddToList(ref membersUnqualified, entity);
                    }
                }		 
	        }
        }


        public DateAndTime.TimeDateYear /* float*/ GetTimeToReachMajority(RatingTypes rating, TierType tier, /*float ratingLevel,*/ int neededVotes, List<Entity> membersBelowRange)
        {
            // compute the time for the last member to fulfill neededVotes to move his principles above ratingLevel.
          
            // time in days..?
            
            // sort the members by lowest distance.
           // var sorted = membersBelowRange.OrderBy(e => tier.Edge - e.PersonEntity.Personality.Principles[rating]);

            // the voters adapt at different speeds. (Adaptability)
            //var neededVoters = sorted.Take(neededVotes);

            // so compute the time needed for all of them, then pick the fastest. The slowest of those will be the returned result

            float tierEdgeBelow = tier.GetTierEdgeBelow();

            List<Tuple<Entity, float>> results = new List<Tuple<Entity, float>>();
            foreach (var item in membersBelowRange)
            {
               // DateAndTime.TimeDateYear time = item.PersonEntity.Personality.GetTimeForPrincipleToReachValue(rating, tier.Edge);
                float time = item.PersonEntity.Personality.GetTimeForPrincipleToReachValue(rating, tierEdgeBelow);

                results.Add(new Tuple<Entity, float>(item, time));
            }


            var sorted = results.OrderBy(t => t.Item2).ToList();


            float highest = sorted[neededVotes - 1].Item2;

            DateAndTime.TimeDateYear timeStruct = DateAndTime.GetSecondsToIngameDays(highest);

            return timeStruct;
            
        }

        public int GetNoOfPersons(Predicate<Entity> filter)
        {
            if (filter == null)
            {
                return Persons.Count(e => e.Site == this.Site); // ??
            }
            else
            {
                return Persons.Count(e => filter(e) == true); // e.Value.Site == this.Site);
            }
        }

        public List<Entity> GetPersons(Predicate<Entity> filter)
        {
            return Persons.Where(e => filter(e)).ToList();

        }

        public Entity GetRandomPerson(Predicate<Entity> filter)
        {
            var persons = GetPersons(filter);

            if (persons != null && persons.Count > 0)
            {
                Entity randomPerson = Common.GetRandomListMember(persons, The.Sim.GameplayRandomGenerator);

                return randomPerson;
            }
            else return null;

        }

        public int GetNoOfPersons()
        {
            return Persons.Count; // noOfPersons;
        }


        public bool IsOverPopulationCap()
        {
            return IsOverPopulationCap(GetNoOfPersons());
        }

        private bool IsOverPopulationCap(int members)
        {
            return members > GameData.Instance.Constants.PopulationCap;
        }

        public bool IsWithinPopulationCap(int additionalMembers)
        {
            return !IsOverPopulationCap(GetNoOfPersons() + additionalMembers);
        }

        //when we want to check within our allegiance aggro radius, might not be needed at all
        /*public ThreatJobManager.ThreatEvaluationStatus IsInAggrevationRange(EntityID anEntityToCheckAggrevationAgainstID, ref bool isInAggrevationRange)
        {
            return IsInAggrevationRange(anEntityToCheckAggrevationAgainstID,ref isInAggrevationRange, RepresentativeEntityType.IntelligenceType.aggroRadius);
        }*/

        public bool WasRecentlyAttackedBy(Entity potentialAttacker)
        {
            EntityID? currentTarget = potentialAttacker.Intelligence.CombatInfo.Target;
            if (currentTarget != null)
            {
                // currently attacking?
                if (Members.Any(
                e => e.EntityID == currentTarget))
                {
                    return true;
                }
            }

            // has recently attacked (hit) us?
            return Members.Any(
                e =>
                    e.Intelligence.Memory.WasRecentlyHitBy(potentialAttacker.EntityID));


            // OLD: tests within aggro range - but this was already done in ScoreNearness

            /*
            if (RepresentativeEntityType.IntelligenceType.AggroRange == null)
            {
                return false;
            }
                     

            List<Entity> allegianceMembersCloseEnoughToBeRelevant = new List<Entity>();
            Predicate<Entity> inAllegianceFilter = (Entity entityToCheck) => { return AllegianceMembers.ContainsValue(entityToCheck); };

            The.AgentQuadTree.GetEntitiesInRange
            (
                SharedKnowledge.GetLocation(potentialAttacker).ToVector2(),
                RepresentativeEntityType.IntelligenceType.AggroRange.Value,
                inAllegianceFilter,
                ref allegianceMembersCloseEnoughToBeRelevant
            );

            foreach (Entity allegianceMember in allegianceMembersCloseEnoughToBeRelevant)
            {
                if (allegianceMember.Intelligence.Memory.WasRecentlyAttackedBy(potentialAttacker.EntityID))
                {
                    return true;
                }
            }
            return false;*/
        }


        public float? GetMaximumAggroRange()
        {
            float? maxRange = Members.Max(e => e.GetAggroRange());

            return maxRange;

        }

        public bool IsOwnedByAllegiance(IKnownEntityData e)
        {
            if (e.OwnedBy != null)
            {
                IOwner owner;

                if (LookUpOwners.ResolveEntityOwner(e, out owner))
                {
                    if (owner != null && owner.Allegiance == this) // The.InGameUI.UIAllegiance)
                        return true;
                }
            }

            return false;
        }

        /*
        public ThreatJobManager.ThreatEvaluationStatus IsInAggrevationRange(EntityID anEntityToCheckAggrevationAgainstID, ref bool isInAggrevationRange, float? aggroRangeToCheck)
        {
           IKnownEntityData entityToCheckAggrevationAgainst;
            SharedKnowledge.GetKnownData(anEntityToCheckAggrevationAgainstID, out entityToCheckAggrevationAgainst);
            if (entityToCheckAggrevationAgainst == null)
            {
                isInAggrevationRange = false;
                return ThreatJobManager.ThreatEvaluationStatus.Done;
            }

            if (aggroRangeToCheck == null)
            {
                isInAggrevationRange = true;//We have infinite aggro range
                return ThreatJobManager.ThreatEvaluationStatus.Done;
            }
            else
            {
                return CheckAggrevationWithRange(ref isInAggrevationRange, aggroRangeToCheck.Value, entityToCheckAggrevationAgainst);
            }  
        }

        private ThreatJobManager.ThreatEvaluationStatus CheckAggrevationWithRange(ref bool isInAggrevationRange, float aggroRangeToCheck, IKnownEntityData entityToCheckAggrevationAgainst)
        {
            List<Entity> allegianceMembersCloseEnoughToBeRelevant = null;
            Predicate<Entity> inAllegianceFilter;
            inAllegianceFilter = (Entity entityToCheck) => { return AllegianceMembers.ContainsKey(entityToCheck); };

            The.AgentQuadTree.GetEntitiesInRange
            (
               // SharedKnowledge.GetLocation(entityToCheckAggrevationAgainst).ToVector2(), // Lars: completely unnecessary to call this...
                entityToCheckAggrevationAgainst.Location.ToVector2(),
                aggroRangeToCheck,
                inAllegianceFilter,
                ref allegianceMembersCloseEnoughToBeRelevant
            );

            if (allegianceMembersCloseEnoughToBeRelevant == null)
            {
                isInAggrevationRange = false;
                return ThreatJobManager.ThreatEvaluationStatus.Done;
            }

            bool isInAggroRangeOfCurrentEnemy = false;
            bool wasNotInAggroRange = false;
            if (allegianceMembersCloseEnoughToBeRelevant.Count() == 0)
            {
                isInAggrevationRange = false;
                return ThreatJobManager.ThreatEvaluationStatus.Done; //Without this processing can go on forever
            }

            foreach (Entity allegianceMember in allegianceMembersCloseEnoughToBeRelevant)
            {
                if (allegianceMember.IsAggrevatedByEntity(entityToCheckAggrevationAgainst, aggroRangeToCheck, ref isInAggroRangeOfCurrentEnemy) == ThreatJobManager.ThreatEvaluationStatus.Done)
                {
                    if (isInAggroRangeOfCurrentEnemy)
                    {
                        isInAggrevationRange = true;
                        return ThreatJobManager.ThreatEvaluationStatus.Done;
                    }
                    else
                    {
                        wasNotInAggroRange = true;
                    }
                }
            }

            if (wasNotInAggroRange)
            {
                return ThreatJobManager.ThreatEvaluationStatus.Done;
            }
            else
            {
                return ThreatJobManager.ThreatEvaluationStatus.Processing;
            }
        }
        */

        /// <summary>
        /// checks if this allegiance operates sufficient communication systems to reach toAllegiance.
        /// Checks that both allegiances can reach each other with the same method of communication.
        /// </summary>
        /// <param name="toAllegiance"></param>
        /// <returns></returns>
      /*  public bool IsInCommunicationRange(ICommunicates toAllegiance, out CommunicationMethod? workingMethod)
        {
            // two allegiances on the same site can always communicate:
            if (Site != null && Site == toAllegiance.Site)
            {
                workingMethod = CommunicationMethod.Direct;
                return true;
            }

            // get the surface distance:
            double distance = The.Sim.World.GetAirDistance(toAllegiance.Site.Coords, this.Site.Coords);

            workingMethod = null;

            if (distance < GameData.Instance.Constants.VisualCommunicationRangeInKms)
            {
                workingMethod = CommunicationMethod.Visual; // NEW: default comm method
                return true;
            }

            CommunicationMethod method;
            Array methods = Enum.GetValues(typeof(CommunicationMethod));
            foreach (var item in methods)
            {
                // test each method...
                method = (CommunicationMethod)item;
                if (this.CanCommunicate(method, distance))
                {
                    // test both ends:
                    if (toAllegiance.CanCommunicate(method, distance))
                    {
                        workingMethod = method;
                        return true;
                    }
                }
            }

            return false;
        }*/

        /// <summary>
        /// tests if the allegiance is running functional (what about manned? powered?) communication equipment that can reach the specified distance
        /// </summary>
        /// <param name="method"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool CanCommunicate(CommunicationMethod method, double distance)
        {
            if (RepresentativeEntityType.CommunicatorType != null) //IntelligenceType.IntrinsicCommunicators != null)
            {
                if (RepresentativeEntityType.CommunicatorType.Method == method
                       && RepresentativeEntityType.CommunicatorType.IsInRange(distance))
                {
                    return true;
                }
               /* foreach (var item in RepresentativeEntityType.IntelligenceType.IntrinsicCommunicators)
                {
                    if (item.Method == method
                        && item.IsInRange(distance))
                    {
                        return true;
                    }
                }*/
            }

            foreach (var expedition in Expeditions)
            {
                foreach (var item in expedition.OwnedEntities.Communicators)
                {
                    foreach (var communicator in item.Value)
                    {
                        // the communicator can not be in FOW, and just be believed to work. Include a sensor with it, same as for terminal... This simulates short range contact from agents...
                        Entity entity = Entity.FindByID(communicator);

                        if (entity != null
                            && entity.EntityType.CommunicatorType.Method == method)
                        {
                            Communicator communicatorComponent;
                            entity.Find(out communicatorComponent);

                            if (communicatorComponent.IsCommunicatorWorkingAndInRange(distance))
                                return true;
                        }

                    }
                }
            }

            return false;
        }




        public bool IsUnderThreat()
        {
            return SharedKnowledge.AllKnownEntities.ThreatJobs.Count > 0; // .Exists(j => ((ThreatJob)j).IsVermin == false);

        }

        public bool MembersHaveAIDisabled()
        {
            foreach (var member in Members)
            {
                if (member.Intelligence.DisableAI == false)
                {
                    return false;
                }
            }

            return true;
        }

        public void Destroy()
        {
            RemoveIDEntry();

            ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).RemoveIDEntry();
            ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).RemoveIDEntry();

            //The.Sim.Site.Allegiances.Remove(this);
            Site.Allegiances.Remove(this);

            if (ThreatAndCombatJobManager != null)
            {
                ThreatAndCombatJobManager.Destroy();
            }

            if (SharedKnowledge != null)
            {
                SharedKnowledge.Destroy();
            }

            foreach (var expedition in Expeditions)
            {
                expedition.Destroy();
            }

            The.Sim.World.RemoveRelation(ID);


            if (RepresentativeEntityType.PolledEvents != null)
            {
                List<PolledEventType> list;
                if (RepresentativeEntityType.PolledEvents.TryGetValue(Scope.Allegiance, out list))
                {
                    foreach (var eventType in list)
                    {
                        Site.EventManager.RemovePolledEvent(eventType, null, null, this.ID);
                    }
                }

            }
        }


        public static void GetMembers(IHasExposedProperties presentedObject, List<IHasExposedProperties> listToFillWithProperties)
        {
            Allegiance allegiance = (Allegiance)presentedObject;

            // make a copy of the list - important - so filtering does not affect the original list:
            listToFillWithProperties.AddRange(allegiance.Members);
        }


        public static void GetPersons(IHasExposedProperties presentedObject, List<IHasExposedProperties> listToFillWithProperties)
        {
            Allegiance allegiance = (Allegiance)presentedObject;

            listToFillWithProperties.AddRange(allegiance.Persons); // make a copy to allow filtering

          /*  foreach (var item in allegiance.Persons)
            {
                listToFillWithProperties.Add(item);               
            }     */       
        }

        public static void GetRandomPersons(IHasExposedProperties presentedObject, ref List<IHasExposedProperties> listToFillWithProperties)
        {
            GetPersons(presentedObject, listToFillWithProperties);

            listToFillWithProperties = Common.Randomize(listToFillWithProperties, The.Sim.GameplayRandomGenerator);
        }

        #region IHasExposedProperties

        //   private static Dictionary<string, UWGame.SimSide.Entities.GetChildren> listFunctions = new Dictionary<string, UWGame.SimSide.Entities.GetChildren>();


        public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget,
            SharedKnowledge getterKnowledge = null)
        {
            //UWGame.SimSide.Entities.GetChildren getListFunction = listFunctions[keyToList];

            switch (keyToList)
            {
                case "members":
                    GetMembers(this, listToFillWithProperties);
                    break;

                case "persons":
                    GetPersons(this, listToFillWithProperties);
                    break;

                case "randomPersons":
                    GetRandomPersons(this, ref listToFillWithProperties);
                    break;
            }

            if (filter != null)
            {
                Site.FilterChildren(listToFillWithProperties, filter, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            }

            //getListFunction.Invoke(this, filter, ref listToFillWithProperties);       
        }

        public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            PropertyResult? result = null;
            PropertyResult customResult;

            if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
            {
                result = exposedPropertyValueFunctions[propertyKey].Invoke(this, getterKnowledge, parent);
            }
            else if (customFields != null && customFields.TryGetValue(propertyKey, out customResult))
            {
                result = customResult;
            }

            return result;
        }

        public static PropertyResult? GetKeyName(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return new ClientSide.PropertyPresentation.PropertyResult() { StringResult = ((Allegiance)anObjectToGetValueFrom).KeyName };
        }

        public static PropertyResult? GetName(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return new ClientSide.PropertyPresentation.PropertyResult() { StringResult = ((Allegiance)anObjectToGetValueFrom).Name };
        }

        public static PropertyResult? GetComfortRating(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Allegiance)anObjectToGetValueFrom).GetRating(getterKnowledge, RatingTypes.Comfort);
        }

        public static PropertyResult? GetSecurityRating(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Allegiance)anObjectToGetValueFrom).GetRating(getterKnowledge, RatingTypes.Security);
        }

        public static PropertyResult? GetFoodRating(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return ((Allegiance)anObjectToGetValueFrom).GetRating(getterKnowledge, RatingTypes.Food);
        }

        private PropertyResult? GetRating(SharedKnowledge getterKnowledge, RatingTypes statType)
        {
            PropertyResult result = new PropertyResult();

            //Statistic stat = ;

            result.NumberResult = Statistics.GetRating(statType); // (float)((Rating)stat).GetLatestValue();

            return result;

        }


        public void SetPropertyValue(string propertyKey, ClientSide.PropertyPresentation.PropertyResult? value)
        {

            Entity.SetPropertyValue(ref customFields, propertyKey, value);

        }

        public string GetCaption(string captionMethodKey)
        {
            throw new NotImplementedException();
        }

        public string GetDefaultCaption(string propertyKey)
        {
            throw new NotImplementedException();
        }

        public void GetDefaultKey(out string PropertyKey)
        {
            throw new NotImplementedException();
        }

        public EntityID? GetEntityID()
        {
            return null;
        }
        public bool GetIsSeenDirectly() //SharedKnowledge getterKnowledge)
        {
            return true;
        }

        #endregion


        public Expedition GetExpedition(string key)
        {
            // Expedition expedition = Expeditions.FirstOrDefault(e => e.Name == name);           
            Expedition expedition = Expeditions.FirstOrDefault(e => e.KeyName == key);

            return expedition;
        }


        public Expedition GetFirstExpedition()
        {
            if (Expeditions.Count > 0)
            {
                return Expeditions[0];
            }

            return null;
        }

        public Allegiance GetAllegiance
        {
            get
            {
                return this;
            }
        }

        public void IterateMembers(Action<Entity> iterateFunction)
        {
            foreach (var item in Members)
            {
                iterateFunction(item);
            }

        }



        public void IterateOwnedItems(Action<EntityGroup> iterateFunction)
        {
            for (int i = Expeditions.Count - 1; i >= 0; i--)
            {
                iterateFunction(Expeditions[i].OwnedEntities);
            }
        }

        #region IHasEntityGroup

        // the EntityGroup is placed in SharedKnowledge isntead of Allegiance...

        Allegiances.Allegiance IHasEntityGroup.Allegiance
        {
            get
            {
                return this;
            }

        }

        /*  private EntityGroup ownerContent; // = new OwnerContent();
          public EntityGroup OwnedEntities
          {
              get
              {
                  return ownerContent;
              }
          }*/

        public Vector3? Location
        {
            get { return Vector3.Zero; } // ??? put in a home/cave/expedition location here...
        }


        /// <summary>
        /// can the entity be consumed, or can a consumable item be extracted from it?
        /// </summary>
        /// <param name="food"></param>
        /// <returns></returns>
        public bool IsEatable(EntityType food)
        {
            return FoodExtraction.IsEatable(food);
        }

        public int NoOfWorkers
        {
            get
            {
                return Members.Count; // ??
            }
        }


        #endregion


        #region ILookup

        private AllegianceID id = AllegianceID.Invalid;
        static AllegianceID IDCounter = AllegianceID.First;

        public AllegianceID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public AllegianceID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= AllegianceID.Max)
            {
                throw new Exception("Astounding, AllegianceID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public AllegianceID SnapshotID(Snapshotter sn, AllegianceID id)
        {
            return (AllegianceID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != AllegianceID.Invalid)
                LookUp<Allegiance, AllegianceID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = AllegianceID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Allegiance, AllegianceID>.Remove(this);
        }

        void ILookUp<Allegiance, AllegianceID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = AllegianceID.First;
        }


        void ILookUp<Allegiance, AllegianceID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection() 
        {
            LookUp<Allegiance, AllegianceID>.Create();
        }

        #endregion

        #region CanIterateEntitiesID ILookup

        CanIterateEntitiesID canIterateEntitiesID;
        CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ID
        {
            get
            {
                return canIterateEntitiesID;
            }
        }

        CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.GetUniqueID()
        {
            return HasMembers.GetUniqueID();
        }



        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.AddToLookup()
        {
            canIterateEntitiesID = ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).GetUniqueID();

            if (canIterateEntitiesID != CanIterateEntitiesID.Invalid)
            {
                LookUpICanIterateEntities.Add(canIterateEntitiesID, this); // uses special class!
            }
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.RemoveIDEntry()
        {
            LookUpICanIterateEntities.Remove(this);  // uses special class!
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.SetInvalid()
        {
            canIterateEntitiesID = CanIterateEntitiesID.Invalid;
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.CreateLookupCollection() // interface method - does nothing...
        {

        }

        #endregion

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion((Snapshotter.Version)2); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // IDCounter = (AllegianceID)sn.DoEnum(IDCounter);
            IDCounter = sn.DoEnum(IDCounter);
            id = SnapshotID(sn, id);
            hasEntityGroupID = sn.DoEnum(hasEntityGroupID);
            canIterateEntitiesID = sn.DoEnum(canIterateEntitiesID);


            this.AllegianceType = sn.DoEnum(AllegianceType);
            this.DebugColor = sn.DoColor(DebugColor);
            this.FoodExtraction = (FoodExtraction)sn.DoISnapshot(FoodExtraction);
            this.HumanActivities = (HumanActivities)sn.DoISnapshot(HumanActivities);
            this.KeyName = sn.DoString(KeyName);
            //     this.Manager = (OtherSiteAllegianceManager)sn.DoISnapshot(Manager); TODO
            this.Name = sn.DoString(Name);          
            this.RepresentativeEntityType = sn.DoGameData(RepresentativeEntityType);
            this.SharedKnowledge = (SharedKnowledge)sn.DoISnapshot(SharedKnowledge);
            this.snapshotThreatManager = sn.SnapshotID<ICyclable, CyclableID>(ThreatAndCombatJobManager);
            this.OtherSiteAllegianceManager = (OtherSiteAllegianceManager)sn.DoISnapshot(OtherSiteAllegianceManager);
            this.Statistics = (GroupStatistics)sn.DoISnapshot(Statistics);
            this.tradeCredits = sn.DoDecimalNullable(tradeCredits);
            this.Policy = (AllegiancePolicy)sn.DoISnapshot(Policy);
            this.AllegiancesWeAreInContactWith = sn.DoHashSet(AllegiancesWeAreInContactWith);
            this.PermitsImmigration = sn.DoBool(PermitsImmigration);          
            this.IndependentMembers = sn.DoList(IndependentMembers);

            snapshotSiteID = (SiteID)sn.SnapshotID<Site, SiteID>(Site);
            snapshotThreatGroupID = sn.SnapshotID<ThreatGroup, ThreatGroupID>(ThreatGroup);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotMembers = Members.Select(c => c.EntityID).ToList();
                snapshotExpeditions = Expeditions.Select(e => e.ID).ToList();
                if (Missions != null)
                {
                    snapshotMissions = Missions.Select(c => c.ID).ToList();
                }
            }

            snapshotMembers = sn.DoList(snapshotMembers);
            snapshotExpeditions = sn.DoList(snapshotExpeditions);
            snapshotMissions = sn.DoList(snapshotMissions);

            this.forageAndHuntingRadius = sn.DoInt32Nullable(forageAndHuntingRadius);

            customFields = sn.DoDictionary(customFields);
          

            sn.Ignore(Missions);
            sn.Ignore(Expeditions);
            sn.Ignore(ThreatAndCombatJobManager);
            sn.Ignore(MembersList);
            sn.Ignore(Members);
            sn.Ignore(exposedPropertyValueFunctions); // handled in static ctor
            sn.Ignore(Persons);

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            // reconnect after load:
            foreach (var item in snapshotMembers)
            {
                Entity entity = Entity.FindByID(item);

                Members.Add(entity);
                MembersList.Add(entity);

                if (entity.EntityType.Person != null)
                {
                    Persons.Add(entity);
                }

            }

            Site = LookUp<Site, SiteID>.FindByID(snapshotSiteID);

            if (snapshotThreatGroupID.HasValue)
            {
                ThreatGroup = LookUp<ThreatGroup, ThreatGroupID>.FindByID(snapshotThreatGroupID.Value);
            }

            if (HumanActivities != null)
            {
                HumanActivities.LoadPostProcess(sn);
            }

            if (SharedKnowledge != null)
            {
                SharedKnowledge.LoadPostProcess(sn);
            }

            if (snapshotThreatManager.HasValue)
            {
                ThreatAndCombatJobManager = (ThreatJobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotThreatManager.Value);
            }

            if (OtherSiteAllegianceManager != null)
            {
                OtherSiteAllegianceManager.LoadPostProcess(sn);
            }

            if (FoodExtraction != null)
            {
                FoodExtraction.LoadPostProcess(sn);
            }

            Statistics.LoadPostProcess(sn);

            Expeditions = snapshotExpeditions.Select(e => Expedition.FindByID(e)).ToList();

            Policy.LoadPostProcess(sn);



            if (snapshotMissions != null)
            {
                Missions = snapshotMissions.Select(t => LookUp<Mission, MissionID>.FindByID(t)).ToList();

                snapshotMissions = null;
            }


        }

        #endregion


        #region HasEntityGroupID ILookup

        HasEntityGroupID hasEntityGroupID;
        HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.ID
        {
            get
            {
                return hasEntityGroupID;
            }
        }

        HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.GetUniqueID()
        {
            return HasEntityGroup.GetUniqueID();
        }


        void ILookUp<IHasEntityGroup, HasEntityGroupID>.AddToLookup()
        {
            hasEntityGroupID = ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).GetUniqueID();

            if (hasEntityGroupID != HasEntityGroupID.Invalid)
            {
                LookUpHasEntityGroup.Add(hasEntityGroupID, this); // uses special class!
            }
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.RemoveIDEntry()
        {
            LookUpHasEntityGroup.Remove(this);  // uses special class!
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.SetInvalid()
        {
            hasEntityGroupID = HasEntityGroupID.Invalid;
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.CreateLookupCollection() // interface method - does nothing... Sim will call 
        {

        }
    

        #endregion
    }



}

using GameStateManagement;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.Control;

namespace UWGame.Steam
{
    /// <summary>
    /// enum, since programmer defined
    /// </summary>
    public enum AchievementID : int
    {
        tutorialCompleted,
        clayPitCompleted,
        headwayCompleted,
        twinklerIslandNoDeaths,
        swarmerKills,
        hideProducer,
        gunsProducer,
        twinklerIslandNoKills,
        fieldsOfTauCetiTrader,
        muckrootNoRefining,
        muckrootMining,
        relatives,
        improviser,
        hunterGatherers,
        claypitRatings,
        headwayAlternative
    };

    /// <summary>
    /// caches achievements and stats on startup
    /// stores achievements, resends them each update until they go through
    /// </summary>
    public class StatsAndAchievements 
    {
       

       // private Achievement_t[] m_Achievements = new Achievement_t[] 

        /// <summary>
        /// we don't get the full list from Steam, only query the status for each of these entries
        /// </summary>
        private Dictionary<AchievementID, Achievement> achievements; /* = new Dictionary<AchievementID, Achievement>()
        {
             { AchievementID.tutorialCompleted,  new Achievement(AchievementID.tutorialCompleted, "Rescued", "")},
             { AchievementID.clayPitCompleted,  new Achievement(AchievementID.clayPitCompleted, "", "")},
             { AchievementID.swarmerKills,  new Achievement(AchievementID.swarmerKills, "Biohazard", "")},
             { AchievementID.swarmerKills,  new Achievement(AchievementID.swarmerKills, "Biohazard", "")},
             { AchievementID.swarmerKills,  new Achievement(AchievementID.swarmerKills, "Biohazard", "")},
             { AchievementID.swarmerKills,  new Achievement(AchievementID.swarmerKills, "Biohazard", "")},
             { AchievementID.swarmerKills,  new Achievement(AchievementID.swarmerKills, "Biohazard", "")},
             { AchievementID.swarmerKills,  new Achievement(AchievementID.swarmerKills, "Biohazard", "")},
             { AchievementID.swarmerKills,  new Achievement(AchievementID.swarmerKills, "Biohazard", "")},
             { AchievementID.swarmerKills,  new Achievement(AchievementID.swarmerKills, "Biohazard", "")}

		  
	    };*/

        // Our GameID
        private CGameID m_GameID;

        // Did we get the stats from Steam?
        private bool m_bRequestedStats;
        private bool m_bStatsValid;

        // Should we store stats this frame?
        private bool m_bStoreStats;

        // Current Stat details
     /*   private float m_flGameFeetTraveled;
        private float m_ulTickCountGameStart;
        private double m_flGameDurationSeconds;

        // Persisted Stat details
        private int m_nTotalGamesPlayed;
        private int m_nTotalNumWins;
        private int m_nTotalNumLosses;
        private float m_flTotalFeetTraveled;
        private float m_flMaxFeetTraveled;
        private float m_flAverageSpeed;*/

        protected Callback<UserStatsReceived_t> m_UserStatsReceived;
        protected Callback<UserStatsStored_t> m_UserStatsStored;
        protected Callback<UserAchievementStored_t> m_UserAchievementStored;

        Controller controller;

        public StatsAndAchievements(Controller controller)
        {
            this.controller = controller;

            achievements = new Dictionary<AchievementID, Achievement>();

            foreach (AchievementID item in Enum.GetValues(typeof(AchievementID)))
            {
                achievements.Add(item, new Achievement(item));
            }

        }


        public void Initialize() // OnEnable()
        {
            if (!controller.SteamManager.IsInitialized)
                return;

            // Cache the GameID for use in the Callbacks
            m_GameID = new CGameID(SteamUtils.GetAppID());

            m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

            // Unity stuff: These need to be reset to get the stats upon an Assembly reload in the Editor.
            m_bRequestedStats = false;
            m_bStatsValid = false;
        }

        public void Update()
        {
            if (!controller.SteamManager.IsInitialized)
                return;

            if (!m_bRequestedStats)
            {
                // Is Steam Loaded? if no, can't get stats, done
              /*  if (!SteamManager.Initialized)
                {
                    m_bRequestedStats = true;
                    return;
                }*/

                // If yes, request our stats
                bool bSuccess = SteamUserStats.RequestCurrentStats();

                // This function should only return false if we weren't logged in, and we already checked that.
                // But handle it being false again anyway, just ask again later.
                m_bRequestedStats = bSuccess;
            }

            if (!m_bStatsValid)
                return;

            // Get info from sources

            // Evaluate achievements
         /*   foreach (Achievement_t achievement in m_Achievements)
            {
                if (achievement.m_bAchieved)
                    continue;

                switch (achievement.m_eAchievementID)
                {
                    case Achievement.ACH_WIN_ONE_GAME:
                        if (m_nTotalNumWins != 0)
                        {
                            UnlockAchievement(achievement);
                        }
                        break;
                    case Achievement.ACH_WIN_100_GAMES:
                        if (m_nTotalNumWins >= 100)
                        {
                            UnlockAchievement(achievement);
                        }
                        break;
                    case Achievement.ACH_TRAVEL_FAR_ACCUM:
                        if (m_flTotalFeetTraveled >= 5280)
                        {
                            UnlockAchievement(achievement);
                        }
                        break;
                    case Achievement.ACH_TRAVEL_FAR_SINGLE:
                        if (m_flGameFeetTraveled >= 500)
                        {
                            UnlockAchievement(achievement);
                        }
                        break;
                }
            }*/

            //Store stats in the Steam database if necessary
            if (m_bStoreStats)
            {
                // already set any achievements in UnlockAchievement

                /*
                // set stats
                SteamUserStats.SetStat("NumGames", m_nTotalGamesPlayed);
                SteamUserStats.SetStat("NumWins", m_nTotalNumWins);
                SteamUserStats.SetStat("NumLosses", m_nTotalNumLosses);
                SteamUserStats.SetStat("FeetTraveled", m_flTotalFeetTraveled);
                SteamUserStats.SetStat("MaxFeetTraveled", m_flMaxFeetTraveled);
                // Update average feet / second stat
                SteamUserStats.UpdateAvgRateStat("AverageSpeed", m_flGameFeetTraveled, m_flGameDurationSeconds);
                // The averaged result is calculated for us
                SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);
                */


                bool bSuccess = SteamUserStats.StoreStats();
                // If this failed, we never sent anything to the server, try
                // again later.
                m_bStoreStats = !bSuccess;
            }
        }

        //-----------------------------------------------------------------------------
        // Purpose: Accumulate distance traveled
        //-----------------------------------------------------------------------------
      /*  public void AddDistanceTraveled(float flDistance)
        {
            m_flGameFeetTraveled += flDistance;
        }*/

        //-----------------------------------------------------------------------------
        // Purpose: Game state has changed
        //-----------------------------------------------------------------------------
      /*  public void OnGameStateChange(EClientGameState eNewState)
        {
            if (!m_bStatsValid)
                return;

            if (eNewState == EClientGameState.k_EClientGameActive)
            {
                // Reset per-game stats
                m_flGameFeetTraveled = 0;
                m_ulTickCountGameStart = Time.time;
            }
            else if (eNewState == EClientGameState.k_EClientGameWinner || eNewState == EClientGameState.k_EClientGameLoser)
            {
                if (eNewState == EClientGameState.k_EClientGameWinner)
                {
                    m_nTotalNumWins++;
                }
                else
                {
                    m_nTotalNumLosses++;
                }

                // Tally games
                m_nTotalGamesPlayed++;

                // Accumulate distances
                m_flTotalFeetTraveled += m_flGameFeetTraveled;

                // New max?
                if (m_flGameFeetTraveled > m_flMaxFeetTraveled)
                    m_flMaxFeetTraveled = m_flGameFeetTraveled;

                // Calc game duration
                m_flGameDurationSeconds = Time.time - m_ulTickCountGameStart;

                // We want to update stats the next frame.
                m_bStoreStats = true;
            }
        }*/

        //-----------------------------------------------------------------------------
        // Purpose: Unlock this achievement
        //-----------------------------------------------------------------------------
        public void UnlockAchievement(AchievementID achievement) //Achievement_t achievement)
        {
            Achievement a = achievements[achievement];

            a.m_bAchieved = true;

            // the icon may change once it's unlocked
            //achievement.m_iIconImage = 0;

            // NEW: prevents crash?
            if (!controller.SteamManager.IsInitialized)
            {
                string title = "Achievement could not be unlocked. Steam error.";

                string message = string.Format("Achievement {0} failed to be unlocked.", achievement.ToString());

                UnclaimedWorld.LogError(message, title);

                return;
            }

            try
            {
                // mark it down
                SteamUserStats.SetAchievement(a.m_eAchievementID.ToString()); // can crash if steam not init'ed

            }
            catch(System.InvalidOperationException ex) // System.InvalidOperationException("Steamworks is not initialized.");
            {
               /* if (ex.Message == "Steamworks is not initialized.") // http://steamcommunity.com/app/284100/discussions/2/152390014787245308/
                {*/
                    // we allow the achieved flag to be set, so we
                    // don't retry every frame if it failed...
                    
                    // we should log this.
                    string title = "Achievement could not be unlocked. Steam error.";

                    string message = string.Format("Achievement {0} failed to be unlocked. Error message: {1}", achievement.ToString(), ex.Message);
                    
                    UnclaimedWorld.LogError(message, title);
                    

                    return;
               // }
            }

            // Store stats end of frame
            m_bStoreStats = true;
        }


        public bool IsAchievementUnlocked(AchievementID achievement)
        {
            Achievement a = achievements[achievement];

            return a.m_bAchieved;
        }

        //-----------------------------------------------------------------------------
        // Purpose: We have stats data from Steam. It is authoritative, so update
        //			our data with those results now.
        //-----------------------------------------------------------------------------
        private void OnUserStatsReceived(UserStatsReceived_t pCallback)
        {
            if (!controller.SteamManager.IsInitialized)
                return;

            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    Console.WriteLine("Received stats and achievements from Steam\n");

                    m_bStatsValid = true;

                    // load achievements
                    foreach (var ach in achievements)
                    {
                        string id = ach.Value.m_eAchievementID.ToString();
                        bool isAchieved;
                        bool isOK = SteamUserStats.GetAchievement(id, out isAchieved); // ach.Value.m_bAchieved);
                        ach.Value.m_bAchieved = isAchieved;

                        if (isOK)
                        {
                            ach.Value.m_strName = SteamUserStats.GetAchievementDisplayAttribute(id, "name");
                            ach.Value.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(id, "desc");
                        }
                        else
                        {
                            Console.WriteLine("SteamUserStats.GetAchievement failed for Achievement " + id + "\nIs it registered in the Steam Partner site?");
                        }
                    }

                    // load stats
                  /*  SteamUserStats.GetStat("NumGames", out m_nTotalGamesPlayed);
                    SteamUserStats.GetStat("NumWins", out m_nTotalNumWins);
                    SteamUserStats.GetStat("NumLosses", out m_nTotalNumLosses);
                    SteamUserStats.GetStat("FeetTraveled", out m_flTotalFeetTraveled);
                    SteamUserStats.GetStat("MaxFeetTraveled", out m_flMaxFeetTraveled);
                    SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);*/
                }
                else
                {
                    Console.WriteLine("RequestStats - failed, " + pCallback.m_eResult);
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Purpose: Our stats data was stored!
        //-----------------------------------------------------------------------------
        private void OnUserStatsStored(UserStatsStored_t pCallback)
        {
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    Console.WriteLine("StoreStats - success");
                }
                else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
                {
                    // One or more stats we set broke a constraint. They've been reverted,
                    // and we should re-iterate the values now to keep in sync.
                    Console.WriteLine("StoreStats - some failed to validate");
                    // Fake up a callback here so that we re-load the values.
                    UserStatsReceived_t callback = new UserStatsReceived_t();
                    callback.m_eResult = EResult.k_EResultOK;
                    callback.m_nGameID = (ulong)m_GameID;
                    OnUserStatsReceived(callback);
                }
                else
                {
                    Console.WriteLine("StoreStats - failed, " + pCallback.m_eResult);
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Purpose: An achievement was stored
        //-----------------------------------------------------------------------------
        private void OnAchievementStored(UserAchievementStored_t pCallback)
        {
            // We may get callbacks for other games' stats arriving, ignore them
            if (pCallback.m_nGameID == (ulong)m_GameID)
            {
                if (pCallback.m_nMaxProgress == 0)
                {
                    Console.WriteLine("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
                }
                else
                {
                    Console.WriteLine("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Purpose: Display the user's stats and achievements
        //-----------------------------------------------------------------------------
      /*  public void Render()
        {
            if (!SteamManager.Initialized)
            {
                GUILayout.Label("Steamworks not Initialized");
                return;
            }

            GUILayout.Label("m_ulTickCountGameStart: " + m_ulTickCountGameStart);
            GUILayout.Label("m_flGameDurationSeconds: " + m_flGameDurationSeconds);
            GUILayout.Label("m_flGameFeetTraveled: " + m_flGameFeetTraveled);
            GUILayout.Space(10);
            GUILayout.Label("NumGames: " + m_nTotalGamesPlayed);
            GUILayout.Label("NumWins: " + m_nTotalNumWins);
            GUILayout.Label("NumLosses: " + m_nTotalNumLosses);
            GUILayout.Label("FeetTraveled: " + m_flTotalFeetTraveled);
            GUILayout.Label("MaxFeetTraveled: " + m_flMaxFeetTraveled);
            GUILayout.Label("AverageSpeed: " + m_flAverageSpeed);

            GUILayout.BeginArea(new Rect(Screen.width - 300, 0, 300, 800));
            foreach (Achievement_t ach in m_Achievements)
            {
                GUILayout.Label(ach.m_eAchievementID.ToString());
                GUILayout.Label(ach.m_strName + " - " + ach.m_strDescription);
                GUILayout.Label("Achieved: " + ach.m_bAchieved);
                GUILayout.Space(20);
            }

            // FOR TESTING PURPOSES ONLY!
            if (GUILayout.Button("RESET STATS AND ACHIEVEMENTS"))
            {
                SteamUserStats.ResetAllStats(true);
                SteamUserStats.RequestCurrentStats();
                OnGameStateChange(EClientGameState.k_EClientGameActive);
            }
            GUILayout.EndArea();
        }*/

        private class Achievement
        {
            public AchievementID m_eAchievementID;
            public string m_strName;
            public string m_strDescription;
            public bool m_bAchieved;

            /// <summary>
            /// other fields are filled from Steam
            /// </summary>
            /// <param name="achievementID"></param>
            public Achievement(AchievementID achievementID)
            {
                m_eAchievementID = achievementID;
                m_bAchieved = false;
            }

            /// <summary>
            /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
            /// </summary>
            /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
            /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
            /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
            public Achievement(AchievementID achievementID, string name, string desc)
            {
                m_eAchievementID = achievementID;
                m_strName = name;
                m_strDescription = desc;
                m_bAchieved = false;
            }
        }
    }
}

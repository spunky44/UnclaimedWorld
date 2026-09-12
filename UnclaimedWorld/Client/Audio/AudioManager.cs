using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.Client.Audio
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Media;
    using UWGame.SimSide.Maps;
    using UWGame.SimSide;
    using UWGame.ClientSide;

    /// <summary>
    /// Manages playback of sounds and music.
    /// </summary>
    public class AudioManager // : GameComponent
    {
        #region Private fields
      //  private ContentManager content;

       // private Dictionary<string, Song> songs = new Dictionary<string, Song>();
      //  private Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();

        private Song currentSong = null;
        private PlayedSound[] playingSounds = new PlayedSound[MaxSounds]; // SoundEffectInstance[] playingSounds = new SoundEffectInstance[MaxSounds];

        Dictionary<SoundData, PlayedSound> groupedSounds = new Dictionary<SoundData,PlayedSound>();
      //  Dictionary<SoundData, List<Tuple<Vector3, float>>> groupSoundSources = new Dictionary<SoundData, List<Tuple<Vector3, float>>>();

        private bool isMusicPaused = false;

        private bool isFadingMusic = false;
        private MusicFadeEffect fadeEffect;


        /// <summary>
        /// the current volume which may be transitioning (fading) to the target volume
        /// 
        /// Setting Volume to 0.0 subtracts 96 dB from the device volume. Setting Volume to 1.0 subtracts 0 dB from the device volume. 
        /// Values in between 0.0f and 1.0f subtract dB from the volume proportionally.
        /// 
        /// 
        /// MediaPlayer.Volume problem:
        ///  This call locked up for me after I solved the .State problem. The solution was to never ever read this property. 
        ///  Assigning to it can be done just before calling MediaPlayer.Play, or when the local state variable described above is MediaState.Playing, 
        ///  and at no other state. If you still want to read the MediaPlayer.Volume property for some reason, don't. 
        ///  Instead, create a local variable in which you store the volume level that you assigned last to MediaPlayer.Volume.
        /// </summary>
        private float CurrentMusicVolume
        {
            get 
            {
                return mediaPlayerVolume;
            }
            set 
            {               
                try
                {
                    MediaPlayer.Volume = value;  // can sometimes throw an exception https://github.com/MonoGame/MonoGame/issues/4298
                    mediaPlayerVolume = value;
                }
                catch (NullReferenceException)
                {
                    // song changed while changing volume (Lars: probably not?)
                }
               /* catch (SharpDXException) // never reported
                {
                    // device not ready
                }*/
            }
        }

        #region

        /// <summary>
        /// cached properties to avoid hangs...
        /// </summary>
        MediaState mediaPlayerState;
        float mediaPlayerVolume;


        #endregion

        // Change MaxSounds to set the maximum number of simultaneous sounds that can be playing.
        private const int MaxSounds = 32;  


        #endregion
       
       

        /// <summary>
        /// Gets the name of the currently playing song, or null if no song is playing.
        /// </summary>
       // public string CurrentSong { get; private set; }

        

        /// <summary>
        /// Gets or sets the volume to play songs. 1.0f is max volume.
        /// </summary>
        public float MusicVolume
        {
            get;
            set;
            /* get { return MediaPlayer.Volume; }
             set { MediaPlayer.Volume = value; }*/
        }

      

        /// <summary>
        /// Gets or sets the master volume for all sounds. 1.0f is max volume.
        /// </summary>
        public float SoundVolume
        {
            get { return SoundEffect.MasterVolume; }
            set { SoundEffect.MasterVolume = value; }
        }

       
        /// <summary>
        /// Gets whether the current song is paused.
        /// </summary>
        public bool IsSongPaused { get { return currentSong != null && isMusicPaused; } }

        bool canPlayMusic;
       
        public AudioManager(Options options, bool canPlayMusic) 
        {

            this.canPlayMusic = canPlayMusic;

            /*MediaPlayer.State problem:
           This call locks up most of the times. The solution that seemed to work for me is quite simple; attach an event to MediaPlayer.MediaStateChanged, 
            * and read out the State there and store it in a local variable inside the audio manager. Do not read out the MediaPlayer.State anywhere else. 
            * Instead, use your local variable storing the state.
            */
            mediaPlayerState = MediaPlayer.State;
            MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged; // we must remember to remove this in Destroy to prevent a memory leak.

            Init(options);

        }


        public void Destroy()
        {
            MediaPlayer.MediaStateChanged -= MediaPlayer_MediaStateChanged;
        }

        void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            mediaPlayerState = MediaPlayer.State;
        }


        /// <summary>
        /// fading requires update to be called
        /// </summary>
        /// <param name="options"></param>
        /// <param name="useFading"></param>
        public void Init(Options options, bool useFading = true) 
        {
            if (options != null)
            {
                if (options.MusicEnabled)
                {
                    MusicVolume = options.MusicVolume; //options.MusicVolume;
                }
                else
                {
                    MusicVolume = 0f;
                }

                if (!useFading)
                {
                    CurrentMusicVolume = MusicVolume;
                }

                if (options.SoundEnabled)
                {
                    SoundVolume = options.SoundFXVolume;
                }
                else
                {
                    SoundVolume = 0f;
                }
                
            }
        }

       

        /// <summary>
        /// Loads a Song into the AudioManager.
        /// </summary>
        /// <param name="songName">Name of the song to load</param>
     /*   public void LoadSong(string songName)
        {
            LoadSong(songName, songName);
        }*/

        /// <summary>
        /// Loads a Song into the AudioManager.
        /// </summary>
        /// <param name="songName">Name of the song to load</param>
        /// <param name="songPath">Path to the song asset file</param>
     /*   public void LoadSong(string songName, string songPath, ContentManager content)
        {
            if (songs.ContainsKey(songName))
            {
                throw new InvalidOperationException(string.Format("Song '{0}' has already been loaded", songName));
            }

            songs.Add(songName, content.Load<Song>(songPath));
        }*/

        /// <summary>
        /// Loads a SoundEffect into the AudioManager.
        /// </summary>
        /// <param name="soundName">Name of the sound to load</param>
      /*  public void LoadSound(string soundName)
        {
            LoadSound(soundName, soundName);
        }*/

        /// <summary>
        /// Loads a SoundEffect into the AudioManager.
        /// </summary>
        /// <param name="soundName">Name of the sound to load</param>
        /// <param name="soundPath">Path to the song asset file</param>
      /*  public void LoadSound(string soundName, string soundPath)
        {
            if (sounds.ContainsKey(soundName))
            {
                throw new InvalidOperationException(string.Format("Sound '{0}' has already been loaded", soundName));
            }

            sounds.Add(soundName, content.Load<SoundEffect>(soundPath));
        }*/

        /// <summary>
        /// Unloads all loaded songs and sounds.
        /// </summary>
       /* public void UnloadContent()
        {
            content.Unload();
        }*/

       

        /// <summary>
        /// Starts playing the song with the given name. If it is already playing, this method
        /// does nothing. If another song is currently playing, it is stopped first.
        /// </summary>
        /// <param name="songName">Name of the song to play</param>
        /// <param name="loop">True if song should loop, false otherwise</param>
        public void PlaySong(Song song, bool loop = false)
        {
            if (!canPlayMusic)
            {
                throw new Exception("Can't play music in this player");
            }

            if ((currentSong != null && currentSong.IsDisposed) || currentSong != song) // currentSong != song threw Excpetion from Song.Equals when currentSong was Disposed = true
            {
                if (currentSong != null)
                {
                    MediaPlayer.Stop();
                }
                
                currentSong = song;

                isMusicPaused = false;
                MediaPlayer.IsRepeating = loop;
                MediaPlayer.Play(currentSong);

            }

           // isMusicPaused = false;
        }

        /// <summary>
        /// Pauses the currently playing song. This is a no-op if the song is already paused,
        /// or if no song is currently playing.
        /// </summary>
        public void PauseSong()
        {
            if (!canPlayMusic)
            {
                return; //throw new Exception("Can't play music in this player");
            }

            if (currentSong != null && !isMusicPaused)
            {
                MediaPlayer.Pause();

                isMusicPaused = true;
            }
        }

        /// <summary>
        /// Resumes the currently paused song. This is a no-op if the song is not paused,
        /// or if no song is currently playing.
        /// </summary>
        public void ResumeSong()
        {
            if (!canPlayMusic)
            {
                return; //throw new Exception("Can't play music in this player");
            }

            if (currentSong != null && isMusicPaused)
            {
                MediaPlayer.Resume();

                isMusicPaused = false;
            }
        }

        /// <summary>
        /// Stops the currently playing song. This is a no-op if no song is currently playing.
        /// </summary>
        public void StopSong()
        {
            if (!canPlayMusic)
            {
                return; //throw new Exception("Can't play music in this player");
            }

            if (currentSong != null && mediaPlayerState != MediaState.Stopped) // MediaPlayer.State != MediaState.Stopped)
            {
                MediaPlayer.Stop();
                isMusicPaused = false;

                currentSong = null;
                
            }
        }

        /// <summary>
        /// Smoothly transition between two volumes.
        /// </summary>
        /// <param name="targetVolume">Target volume, 0.0f to 1.0f</param>
        /// <param name="duration">Length of volume transition</param>
        public void FadeSong(float targetVolume, TimeSpan duration, bool pauseAtEnd)
        {
            if (!canPlayMusic)
            {
                return; //throw new Exception("Can't play music in this player");
            }

            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentException("Duration must be a positive value");
            }

            fadeEffect = new MusicFadeEffect(CurrentMusicVolume /*MediaPlayer.Volume*/, targetVolume, duration, pauseAtEnd);
            isFadingMusic = true;
        }

       

       

        /// <summary>
        /// applies panning etc. to imitate a sound coming from the environment
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="location"></param>
        public SoundEffectInstance PlayWorldSound(SoundData sound, WorldLocation location, float fading = 1f, bool looping = false, Action<PlayedSound> soundEndedCallback = null)
        {
            float pan, distanceFactor;
            GetSoundPanAndDistanceFactor(location.ToVector3(), out pan, out distanceFactor);                       

            return PlaySound(sound, distanceFactor, fading, pan, looping, soundEndedCallback);
        }

        public string PrintSoundsForDebug()
        {
            StringBuilder text = new StringBuilder();
            foreach (var item in playingSounds)
            {
                if (item != null)
                {
                    text.AppendLine(item.ToString());
                }
                else
                {
                    text.AppendLine("null");
                }
            }

            return text.ToString();

        }

        private int GetSoundIndex(SoundData sound, SoundEffectInstance instance)
        {
            int index = Array.FindIndex(playingSounds, s => 
                            s != null 
                            && s.SoundData == sound
                            && s.SoundEffectInstance == instance);

            return index;
        }

        /// <summary>
        /// Plays the sound of the given name with the given parameters.
        /// Will return null if more than 32 sounds are playing!
        /// </summary>
        /// <param name="soundName">Name of the sound</param>
        /// <param name="volume">Volume, 0.0f to 1.0f</param>
        /// <param name="pitch">Pitch, -1.0f (down one octave) to 1.0f (up one octave)</param>
        /// <param name="pan">Pan, -1.0f (full left) to 1.0f (full right)</param>
        public SoundEffectInstance PlaySound(SoundData sound, float distanceFactor, float fading = 1f, float pan = 0f, bool loop = false, Action<PlayedSound> soundEndedCallback = null)
        {
            int index;

            // group sounds here. 
            // skip if already being played, but
            // should return a reference to the current instance being played
            if (sound.PlayMaxOneInstance)
            {
                PlayedSound playedSound;
                if (groupedSounds.TryGetValue(sound, out playedSound))
                {
                    if (soundEndedCallback != null)
                    {
                        // all 'users' of the sound instance should be notified when it ends
                        playedSound.SoundEndedEvent += soundEndedCallback;
                    }

                    // restart the sound if it was just ended:
                    if (playedSound.SoundEffectInstance.State != SoundState.Playing)
                    {
                        playedSound.SoundEffectInstance.Play();
                    }

                    index = GetSoundIndex(sound, playedSound.SoundEffectInstance);

                    if (index < 0)
                    {
                        // reassign the sound if it was just removed:
                        index = GetAvailableSoundIndex();

                        if (index != -1)
                        {
                            playingSounds[index] = playedSound;
                        }
                        else return null;
                    }

                    return playedSound.SoundEffectInstance;
                }
            }


            index = GetAvailableSoundIndex();

            if (index != -1)
            {
                float pitch = 0f;
                if (sound.RandomPitchChange != null)
                {
                    pitch = (float)sound.RandomPitchChange.GetRandomValue(The.Client.ClientRandomGenerator);
                }

                SoundEffectInstance instance = sound.SoundEffect.CreateInstance();
                                
                //sound.
                PlayedSound playedSound = new PlayedSound() { SoundEffectInstance = instance, SoundData = sound /*, SoundEndedCallback = soundEndedCallback*/ };
                playedSound.SoundEndedEvent += soundEndedCallback;

                playingSounds[index] = playedSound;
                SetSoundVolume(instance, sound.Volume, fading, distanceFactor, pan);
                instance.Pitch = pitch;             
                instance.IsLooped = loop;
                instance.Play();

                if (sound.PlayMaxOneInstance)
                {
                    // store it...
                    groupedSounds.Add(sound, playedSound);
                }


                return instance;
              
            }

            return null;
        }


        /// <summary>
        /// the public method should not allow grouped sounds to be stopped.
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="soundInstance"></param>
        public void StopSound(SoundData sound, SoundEffectInstance soundInstance)
        {
            if (sound.PlayMaxOneInstance)
                return;

            int index = index = GetSoundIndex(sound, soundInstance);          

            if (index >= 0)
            {
                PlayedSound playedSound = playingSounds[index];

                StopSound(playedSound);              
            }
            
            
           // groupedSounds.Remove(sound);
        }


        private void StopSound(PlayedSound sound) //SoundData sound, SoundEffectInstance soundInstance)
        {
            if (sound != null)
            {
                sound.SoundEffectInstance.Stop(true);
            }

            // we have to set to null directly, because Update() would trigger a restart of a new random sound in the set.
            int index = GetSoundIndex(sound.SoundData, sound.SoundEffectInstance); // Array.FindIndex(playingSounds, p => p == sound);
            
            if (index >= 0)
            {
                playingSounds[index] = null;
            }
        }

        /// <summary>
        /// Stops all currently playing sounds.
        /// </summary>
        public void StopAllSounds()
        {
            for (int i = 0; i < playingSounds.Length; ++i)
            {
                PlayedSound sound = playingSounds[i];
                if (sound != null)
                {
                    sound.SoundEffectInstance.Stop();
                    sound.SoundEffectInstance.Dispose();
                    playingSounds[i] = null;
                }
            }
        }

        public void SetSoundLocation(SoundEffectInstance instance, SoundData sounddata, float fading, Vector3 location)
        {
            float pan, distanceFactor;
            GetSoundPanAndDistanceFactor(location, out pan, out distanceFactor);

            if (sounddata.PlayMaxOneInstance)
            {
                // use the average of the users' locations:
                Common.AddToList(ref groupedSounds[sounddata].Sources, new Tuple<Vector3, float>(location, fading));
                
                // set the average at the end of Update.
            }
            else
            {
                // Set values directly:
                SetSoundVolumeAndPan(instance, sounddata, fading, distanceFactor, pan);
            }
        }
       

        public static void SetSoundVolumeAndPan(SoundEffectInstance instance, SoundData sounddata, float fading, float distanceFactor, float pan)
        {
            SetSoundVolume(instance, sounddata.Volume, fading, distanceFactor, pan);           
        }

        public static void SetSoundVolume(SoundEffectInstance instance, float sounddataVolume, float fading, float distanceFactor, float pan)
        {
            // the master volume (SoundEffect.MasterVolume) is already set via SoundVolume!
            instance.Volume = sounddataVolume * fading * distanceFactor; /*The.Client.ScreenManager.UserSettings.SoundFXVolume **/
            instance.Pan = pan;
        }


        const float centerRadiusForMaxVolume = 500f;            //500f
        const float centerRadiusForMinVolume = 850f;            //1000f
        const float minVolumeFactor = 0.2f;                     //0.30f

        private static void GetSoundPanAndDistanceFactor(Vector3 location, out float pan, out float distanceVolumeFactor)
        {
            float xLocationOnScreen = location.X - The.MapUI.MapWindowWorldPosition.X;
            float x = Common.Clamp(xLocationOnScreen, 0, The.MapUI.mapWindowWidth);
            x = x / The.MapUI.mapWindowWidth; // 0 - 1
            x = x - 0.5f;  // -0.5 - 0.5
            x = 2f * x; // -1 - 1

            pan = x;

            Vector2 centerOfWindow = The.MapUI.MapWindowWorldPosition + new Vector2(The.MapUI.mapWindowWidth * 0.5f, The.MapUI.mapWindowHeight * 0.5f);
            float distanceToCenter = Common.DistanceOctile(centerOfWindow, location.ToVector2());

            if (distanceToCenter > centerRadiusForMaxVolume)
            {
                // clamp & scale
                float lerpAmount = distanceToCenter - centerRadiusForMaxVolume;

                lerpAmount = lerpAmount / (centerRadiusForMinVolume - centerRadiusForMaxVolume);
                lerpAmount = Common.Clamp(lerpAmount, 0f, 1f);

                distanceVolumeFactor = MathHelper.Lerp(1f, minVolumeFactor, lerpAmount);
            }
            else
            {
                distanceVolumeFactor = 1f;
            }
        }

      
        public void Update(GameTime gameTime)
        {
            UpdateGroupedSounds();

            UpdateEndedSounds();

           

              UpdateMusic(gameTime);   // #HANG  

        }

        private void UpdateEndedSounds()
        {
            PlayedSound playingSound;
            for (int i = 0; i < playingSounds.Length; ++i)
            {
                playingSound = playingSounds[i];

                if (playingSound != null && playingSound.SoundEffectInstance.State == SoundState.Stopped)
                {
                    //   playingSounds[i].Dispose(); // don't call this... it leaves the instance in a compromised state
                    playingSounds[i] = null;

                    playingSound.SoundEnded();

                }
            }
        }

        private void UpdateMusic(GameTime gameTime)
        {
            /*
            * Cause of "not responding" state when task switching in Steam:
            * 
            MediaPlayer.State problem:
           This call locks up most of the times. The solution that seemed to work for me is quite simple; attach an event to MediaPlayer.MediaStateChanged, 
            * and read out the State there and store it in a local variable inside the audio manager. Do not read out the MediaPlayer.State anywhere else. 
            * Instead, use your local variable storing the state.

           MediaPlayer.Volume problem:
           This call locked up for me after I solved the .State problem. The solution was to never ever read this property. 
            * Assigning to it can be done just before calling MediaPlayer.Play, or when the local state variable described above is MediaState.Playing, 
            * and at no other state. If you still want to read the MediaPlayer.Volume property for some reason, don't. 
            * Instead, create a local variable in which you store the volume level that you assigned last to MediaPlayer.Volume.

           These workarounds stopped all forms of "locking up" using my audio engine, 
            * allowing the game to just continue running and not lock up for 5 seconds while the music plays.
             
            */

            if (currentSong != null && mediaPlayerState == MediaState.Stopped) // MediaPlayer.State == MediaState.Stopped)
            {
                currentSong = null;
                isMusicPaused = false;
            }

            if (isFadingMusic && !isMusicPaused)
            {
                if (currentSong != null && mediaPlayerState == MediaState.Playing) // MediaPlayer.State == MediaState.Playing)
                {
                    if (fadeEffect.Update(gameTime.ElapsedGameTime))
                    {
                        isFadingMusic = false;

                        if (fadeEffect.PauseWhenFinished)
                        {
                            PauseSong();
                        }
                    }

                   
                    CurrentMusicVolume = fadeEffect.GetVolume(); 
                }
                else
                {
                    isFadingMusic = false;
                }
            }
        }

        private void UpdateGroupedSounds()
        {
            // average between shared sources of a sound instance:
            List<PlayedSound> soundsWithoutSources = null;
            foreach (var item in groupedSounds) // groupSoundSources)
            {
                if (item.Value.Sources != null && item.Value.Sources.Count > 0)
                {
                    Vector3 averageLocation = Vector3.Zero;
                    // float averageFading = 0f;
                    float totalFading = 0f;

                    foreach (var item2 in item.Value.Sources)
                    {
                        averageLocation += item2.Item1 / (float)item.Value.Sources.Count;
                        //averageFading += item2.Item2 / (float)item.Value.Count;
                        totalFading += item2.Item2;
                    }

                    // clamp the fading..
                    totalFading = Common.ClampTop(totalFading, 1f);

                    float pan, distanceFactor;
                    GetSoundPanAndDistanceFactor(averageLocation, out pan, out distanceFactor);

                    PlayedSound playedSound = groupedSounds[item.Key];
                    SetSoundVolumeAndPan(playedSound.SoundEffectInstance, playedSound.SoundData, totalFading, distanceFactor, pan);


                    item.Value.Sources.Clear(); // clear for next update.
                }
                else
                {
                    // stop grouped sounds that were not updated with any location:
                    // don't stop the sounds. fade to 0 instead...
                    Common.AddToList(ref soundsWithoutSources, item.Value);                    
                }
            }

            if (soundsWithoutSources != null)
            {
                foreach (var item in soundsWithoutSources)
                {
                    // don't stop the sounds. fade to 0 instead...
                    //StopSound(item);
                    //groupedSounds.Remove(item.SoundData);
                    SetSoundVolumeAndPan(item.SoundEffectInstance, item.SoundData, 0f, 0f, 0f);

                }                
            }
        }

        public int MusicFadeInMilliseconds = 800;

        public void Resume()
        {           
            for (int i = 0; i < playingSounds.Length; ++i)
            {
                PlayedSound sound = playingSounds[i];
                if (sound != null && sound.SoundEffectInstance.State == SoundState.Paused)
                {
                    sound.SoundEffectInstance.Resume();
                }
            }

            ResumeSong();
            FadeSong(MusicVolume, new TimeSpan(0, 0, 0, 0, MusicFadeInMilliseconds), false);
 
     /*
            if (!isMusicPaused)
            {
                MediaPlayer.Resume();
            } */          

        }

        public void Pause()
        {           
            for (int i = 0; i < playingSounds.Length; ++i)
            {
                PlayedSound sound = playingSounds[i];
                if (sound != null && sound.SoundEffectInstance.State == SoundState.Playing)
                {
                    sound.SoundEffectInstance.Pause();
                }
            }

            FadeSong(0f, new TimeSpan(0, 0, 0, 0, MusicFadeInMilliseconds), true);
 
          //  MediaPlayer.Pause();            

        }

        // Pauses all music and sound if disabled, resumes if enabled.
      /*  protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            if (Enabled)
            {
                for (int i = 0; i < playingSounds.Length; ++i)
                {
                    if (playingSounds[i] != null && playingSounds[i].State == SoundState.Paused)
                    {
                        playingSounds[i].Resume();
                    }
                }

                if (!isMusicPaused)
                {
                    MediaPlayer.Resume();
                }
            }
            else
            {
                for (int i = 0; i < playingSounds.Length; ++i)
                {
                    if (playingSounds[i] != null && playingSounds[i].State == SoundState.Playing)
                    {
                        playingSounds[i].Pause();
                    }
                }

                MediaPlayer.Pause();
            }

            base.OnEnabledChanged(sender, args);
        }
        */
        // Acquires an open sound slot.
        private int GetAvailableSoundIndex()
        {
            for (int i = 0; i < playingSounds.Length; ++i)
            {
                if (playingSounds[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        #region MusicFadeEffect
        private struct MusicFadeEffect
        {
            public float SourceVolume;
            public float TargetVolume;

            private TimeSpan time;
            private TimeSpan duration;

            public bool PauseWhenFinished;

            public MusicFadeEffect(float sourceVolume, float targetVolume, TimeSpan duration, bool pauseWhenFinished)
            {
                SourceVolume = sourceVolume;
                TargetVolume = targetVolume;
                time = TimeSpan.Zero;
                this.duration = duration;
                this.PauseWhenFinished = pauseWhenFinished;
            }

            public bool Update(TimeSpan deltaTime)
            {
                this.time += deltaTime;

                if (this.time >= duration)
                {
                    this.time = duration;
                    return true;
                }

                return false;
            }

            public float GetVolume()
            {
                return MathHelper.Lerp(SourceVolume, TargetVolume, (float)time.Ticks / duration.Ticks);
            }
        }
        #endregion


        
    }


    public class PlayedSound
    {
        public SoundEffectInstance SoundEffectInstance;

        public SoundData SoundData;

        //  public Action<SoundData> SoundEndedCallback;

        public List<Tuple<Vector3, float>> Sources;

        /// <summary>
        /// memory leak here, a reference to renderable survives after save/load
        /// </summary>
        private Action<PlayedSound> soundEndedEvent;
        public event Action<PlayedSound> SoundEndedEvent
        {
            add
            {
                if (soundEndedEvent == null || !soundEndedEvent.GetInvocationList().Contains(value))
                {
                    soundEndedEvent += value;
                }
            }
            remove
            {
                soundEndedEvent -= value;
            }
        }

        public override string ToString()
        {
            string text = SoundData.Sound;

            if (SoundEffectInstance != null)
            {
                text += " - " + SoundEffectInstance.State + ", vol. " + SoundEffectInstance.Volume.ToString("N2") + ", pan: " + SoundEffectInstance.Pan.ToString("N2");
            }

            return text;
        }

        internal void SoundEnded()
        {
            if (soundEndedEvent != null)
            {
                soundEndedEvent(this); // SoundEffect);
            }
        }
    }

    /// <summary>
    /// Options for AudioManager.CancelFade
    /// </summary>
    public enum FadeCancelOptions
    {
        /// <summary>
        /// Return to pre-fade volume
        /// </summary>
        Source,
        /// <summary>
        /// Snap to fade target volume
        /// </summary>
        Target,
        /// <summary>
        /// Keep current volume
        /// </summary>
        Current
    }
}

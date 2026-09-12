using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using System.Threading.Tasks;
using UWGame.Control;
using UWGame.SimSide.Systems;
using System.Diagnostics;
namespace UWGame.ClientSide.Particles
{
    [DebuggerDisplay("{ParticleSystem.ParticleSystemType.KeyName} {activeParticles.Count}")]
    public class ParticleEmitter : ISleepingUpdatable //IHasOptionalExpiryTimePoint
    {
        public ParticleSystem System;


        private List<Particle> activeParticles = new List<Particle>();

        /// <summary>
        /// Always filled! 
        /// </summary>
        public Renderable Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// the expiry time point
        /// </summary>
        public double? ExpiryTimePointInSeconds { get; set; }

        private double? timePointInSeconds;
        public double? TimePointInSeconds
        {
            get
            {
                return timePointInSeconds;
            }            
        }

        public void SetNextTimepoint(double? timepoint)
        {
            this.timePointInSeconds = timepoint;
        }
       

        private double? updateInterval;
        public double? UpdateInterval
        {
            get
            {
                return updateInterval;
            }

            private set
            {
                if (!Common.IsEqual(updateInterval, value))
                {
                    updateInterval = value;

                    SleepyUpdater<ParticleEmitter> updater = LookUpSleepyUpdater<ParticleEmitter>.FindByID(SleepyUpdater);
                    if (updater != null)
                    {
                        updater.NotifyUpdateIntervalChanged(this);
                    }

                    /*
                    if (UpdateIntervalChanged != null)
                    {
                        // notify the SleepyUpdater
                        UpdateIntervalChanged.Invoke(this);
                    }*/
                }
            }

        }

        void ISleepingUpdatable.CreateSleepyLookupCollection()
        {
            
        }

        public static void CreateSleepyLookupCollection()
        {
            LookUpSleepyUpdater<ParticleEmitter>.Create();
        }

        public SleepyUpdaterID SleepyUpdater { get; set; }


        /// <summary>
        /// an offset to the parent location that we can use when attached
        /// </summary>
        public Vector2 Offset;

        private Vector2 position;
        public Vector2 Position
        {
            get
            {
                if (Parent != null)
                {
                    if (Parent.RenderAsModel != null)
                    {
                        return Parent.RenderAsModel.Location.ToVector2() + Offset;
                    }
                    else
                    {
                        return Parent.Location.Value.ToVector2() + Offset; //Parent.Entity.Location.ToVector2() + Offset;
                    }
                }

                return position;
            }

            set
            {
                position = value;
            }
        }


        public bool EmitInRenderablesFacingDirection // = false;
        {
            get;
            private set;
        }

        public float? EmitDirection;

        /// <summary>
        /// for projectiles that we want to emit in a specific direction
        /// </summary>
        public float? ParentEmitDirection
        {
            get
            {
                if (Parent != null)
                {
                    if (Parent.Entity != null)
                    {
                        return Parent.Entity.FacingAngleWithRotator;

                        //return Parent.Entity.Rotation; //.FacingNormal; // +Offset;
                    }
                    else
                    {
                        return null; 
                    }
                }

                return null; // emitDirection;
            }
            /*
            set
            {
                emitDirection = value;
            }*/
        }

        /// <summary>
        /// ??
        /// </summary>
        public float Intensity;

        public float EmitterScale = 1f;


      
        /// <summary>
        /// this number can come from the particle system or can be overridden by callers, so each emitter will have its own property.
        /// </summary>
        public float? TimeBetweenEmitting = null; 

       // public float TimeBetweenEmitting = 2f; 

      
        public double? NextEmissionTimePoint;


        public ParticleEmitter(ParticleSystem system, Renderable parent, Vector2 worldPosition, Vector2 offset, float? timeBetweenEmitting, bool emitParticlesInParentsDirection)
        {
            this.System = system;
            this.Offset = offset;
            this.Position = worldPosition + Offset;
            this.TimeBetweenEmitting = timeBetweenEmitting;
            this.EmitInRenderablesFacingDirection = emitParticlesInParentsDirection;
            this.Parent = parent;

            // NEW: always add some particles right at start, unless we are in manual mode
            if (!System.ParticleSystemType.EmitParticlesManually)
            {
                AddParticles(EmitterScale, Position);
            }

            // set next emission timepoint:
            SetNextEmissionTimepoint();

          /*  if (!System.ParticleSystemType.EmitParticlesManually 
                && System.ParticleSystemType.ParticlesNeverExpire)
            {
                // only add particles once if they can't expire:
                AddParticles(EmitterScale, Position);
            }
           

            if (EmitsParticlesContinually)
            {
                // set next emission timepoint to begin now:
                NextEmissionTimePoint = UpdateTimePoints.ComputeTimePointFromInterval(0); //TimeBetweenEmitting);
            }
            */

            RecomputeUpdateInterval();
        }

        private bool EmitsParticlesContinually
        {
            get
            {
                return !System.ParticleSystemType.ParticlesNeverExpire
                    && !System.ParticleSystemType.EmitParticlesManually;
            }
        }

        /*
        public void AddParticles(float scale, Vector2 where)
        {
            AddParticles(scale, where, 1f);
        }*/

        /// <summary>
        /// AddParticles's job is to add an effect somewhere on the screen. If there 
        /// aren't enough particles in the freeParticles queue, it will use as many as 
        /// it can. This means that if there not enough particles available, calling
        /// AddParticles will have no effect.
        /// </summary>
        /// <param name="where">where the particle effect should be created</param>
        public void AddParticles(float scale, Vector2 where, float lifetimeFactor = 1f)
        {
           
            // the number of particles we want for this effect is a random number
            // somewhere between the two constants specified by the subclasses.
            int numParticles =
                The.Client.ClientRandomGenerator.Next(System.ParticleSystemType.MinNumParticles, System.ParticleSystemType.MaxNumParticles + 1, "ParticleEmitter", false);

            // create that many particles, if you can.
            for (int i = 0; i < numParticles && System.FreeParticles.Count > 0; i++)
            {
                // grab a particle from the freeParticles queue, and Initialize it.
                Particle p = System.FreeParticles.Dequeue();
                activeParticles.Add(p);

                float? emitDirection;
                if (EmitInRenderablesFacingDirection)
                {
                    emitDirection = ParentEmitDirection.Value;
                }
                else
                {
                    emitDirection = EmitDirection;
                }

                System.InitializeParticle(p, scale, where, lifetimeFactor, emitDirection, null);
            }
        }


        public void UpdateParticlesInParallel(GameTime gameTime)
        {
            foreach (var p in activeParticles)
            {
                p.Update(gameTime);
            }

           // Parallel.ForEach(activeParticles, p => p.Update(gameTime));

        }


        /// <summary>
        /// must be done sequentially because of the shared list operations...
        /// 
        /// must be done each frame... but perhaps we can skip those that are outside the field of view?
        /// </summary>
        public void Update(GameTime gameTime, out bool wasDestroyed)
        {
          
            UpdateEmission();

            UpdateParticleExpiry(out wasDestroyed);

            if (wasDestroyed)
                return;
            
            // expiry of the emitter itself:
            UpdateExpiry(out wasDestroyed);

           
        }

        

        private void UpdateParticleExpiry(out bool wasDestroyed)
        {
            wasDestroyed = false;

         
            // expire particles:
            if (!System.ParticleSystemType.ParticlesNeverExpire)
            {
                for (int i = activeParticles.Count - 1; i >= 0; i--)
                {
                    Particle p = activeParticles[i];

                    // put them onto the free particles
                    // queue.
                    if (!p.IsActive)
                    {
                        activeParticles.RemoveAt(i);

                        System.FreeParticles.Enqueue(p);
                    }

                }

                // see if this emitter should go away:
                if (activeParticles.Count == 0 && System.ParticleSystemType.RemoveEmittersAfterLastParticleExpires)
                {
                    Destroy();

                    wasDestroyed = true;
                }
            }
        }

        private void UpdateEmission()
        {
           if (EmitsParticlesContinually            
                && NextEmissionTimePoint.HasValue)
            {
                if (The.Sim.TimepointReached(NextEmissionTimePoint.Value))
                {                  
                    // add more particles
                    AddParticles(EmitterScale, Position); 
                    
                    // set next emission timepoint:
                    SetNextEmissionTimepoint();
                }
            }

        }

        private void SetNextEmissionTimepoint()
        {
            if (!System.ParticleSystemType.EmitParticlesManually
                && TimeBetweenEmitting.HasValue)
            {
                NextEmissionTimePoint = UpdateTimePoints.ComputeTimePointFromInterval(TimeBetweenEmitting.Value);
            }
            else
            {
                NextEmissionTimePoint = null; // sleep, until the particles expire and the emitter is removed
            }
        }

        private void UpdateExpiry(out bool wasDestroyed)
        {
           
            wasDestroyed = false;

            if (this.ExpiryTimePointInSeconds.HasValue)
            {
                if (The.Sim.TimepointReached(ExpiryTimePointInSeconds.Value))
                {
                    Destroy(); 
                    wasDestroyed = true;

                    ExpiryTimePointInSeconds = null;
                }
                else
                {
                    // wake us up when it is time to die...
                    // don't clamp to zero... better to overshoot a bit.
                    // timeBeforeNextUpdate = Common.ClampBottom(this.ExpiryTimePointInSeconds.Value - The.Sim.TotalUnPausedGameTimeInSeconds, 0.01d); 
                }
            }
        }

        private void RecomputeUpdateInterval()
        {
            bool intervalChanged;
            RecomputeUpdateInterval(out intervalChanged);
        }

        /// <summary>
        /// Remember to keep this up-to-date when more functionality is added to the Update method!
        /// Otherwise performance will suffer because of unnecessary updates, or the object may not receive any Update calls when it needs it.
        /// </summary>
        public void RecomputeUpdateInterval(out bool intervalWasChanged)
        {
            intervalWasChanged = false;

            double? tempInterval = null, currentInterval = null;

            tempInterval = GetParticleExpiryUpdateInterval();
            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

            tempInterval = GetEmissionUpdateInterval();
            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);

            tempInterval = GetEmitterExpiryUpdateInterval(); // UpdateTimePoints.ComputeIntervalFromTimepoint(this.ExpiryTimePointInSeconds);
            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);


            if (!Common.IsEqual(UpdateInterval, currentInterval))
            {
                UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                intervalWasChanged = true;
            }
        }


        /// <summary>
        /// NEW - controls the individual particles and destroys them if expired. Particles cannot sleep...
        /// </summary>
        /// <returns></returns>
        private double? GetParticleExpiryUpdateInterval()
        {
            if (System.ParticleSystemType.ParticlesNeverExpire)
            {
                return null; // sleep - never update
            }
            else
            {
                return 0; // check the particles each frame if they should expire
            }
        }

        private double? GetEmitterExpiryUpdateInterval()
        {
            return UpdateTimePoints.ComputeIntervalFromTimepoint(this.ExpiryTimePointInSeconds);
        }

        private double? GetEmissionUpdateInterval()
        {
            if (System.ParticleSystemType.ParticlesNeverExpire
                || System.ParticleSystemType.EmitParticlesManually)
            {
                return null; // sleep - never emit
            }
            else
            {
                return UpdateTimePoints.ComputeIntervalFromTimepoint(NextEmissionTimePoint);
            }
        }
               


        public void Destroy()
        {
            System.RemoveEmitter(this);

            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                Particle p = activeParticles[i];

                // put them onto the free particles
                // queue.
              
                activeParticles.RemoveAt(i);

                System.FreeParticles.Enqueue(p);              

            }

        }
       

        /// <summary>
        /// only way to draw particles. An emitter is always inside a renderable!
        /// </summary>      
        public void Draw(Color tint)
        {
            if (The.Client.spriteBatch == null)
                return;

            // tell sprite batch to begin, using the spriteBlendMode specified in
            // initializeConstants
            //UWGame.SimSide.Instance.spriteBatch.Begin(spriteBlendMode); // XNA 3
            The.Client.spriteBatch.Begin(0, System.ParticleSystemType.spriteBlendState);

            Vector2 mapWindowPos = The.MapUI.MapWindowWorldPosition;
            Vector2 particleScreenPos;

            Rectangle? sourceRectangle;
            Texture2D texture;
            Vector2 origin;

            foreach (Particle p in activeParticles)
            {
                
                // TODO: move Draw to Particle class

                particleScreenPos = p.Position - mapWindowPos;

                /*    if (The.MapUI.ScreenPositionIsVisible(particleScreenPos, 10))
                    {*/

                // normalized lifetime is a value from 0 to 1 and represents how far
                // a particle is through its life. 0 means it just started, .5 is half
                // way through, and 1.0 means it's just about to be finished.
                // this value will be used to calculate alpha and scale, to avoid 
                // having particles suddenly appear or disappear.
                float alpha;
                Color color;
                float scale;

                if (p.Lifetime.HasValue)
                {
                    float normalizedLifetime = p.TimeSinceStart / p.Lifetime.Value;

                    // we want particles to fade in and fade out, so we'll calculate alpha
                    // to be (normalizedLifetime) * (1-normalizedLifetime). this way, when
                    // normalizedLifetime is 0 or 1, alpha is 0. the maximum value is at
                    // normalizedLifetime = .5, and is
                    // (normalizedLifetime) * (1-normalizedLifetime)
                    // (.5)                 * (1-.5)
                    // .25
                    // since we want the maximum alpha to be 1, not .25, we'll scale the 
                    // entire equation by 4.
                    alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);
                    // Color color = new Color(startColor.R, startColor.G, startColor.B, (byte)(255*alpha)); // xna 3
                    color = System.ParticleSystemType.StartColor * alpha;  // xna 4

                    // make particles grow as they age. they'll start at 75% of their size,
                    // and increase to 100% once they're finished.
                    scale = p.Scale * (.75f + .25f * normalizedLifetime);

                }
                else
                {
                    alpha = 1f;
                    color = System.ParticleSystemType.StartColor;
                    scale = 1f;
                }

                
            
                if (p.Player == null)
                {
                    sourceRectangle = null;
                    texture = System.ParticleSystemType.Texture;
                    origin = System.ParticleSystemType.Origin;
                   
                }
                else
                {                      
                    sourceRectangle = p.Player.GetFrame();
                    texture = p.Player.Animation.Texture;
                    color = p.Player.GetCurrentColor(null);  // TODO: perhaps move the color player to Renderable Effect
                    origin = p.Player.Animation.Origin;
                }

                color = new Color(color.ToVector4() * tint.ToVector4());

                The.Client.spriteBatch.Draw(texture, particleScreenPos, sourceRectangle, color,
                       p.Rotation, origin, scale, SpriteEffects.None, 0.0f);

            }         

            The.Client.spriteBatch.End();

        }


      
       

    }
}

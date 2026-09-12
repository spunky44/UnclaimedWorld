#region File Description
//-----------------------------------------------------------------------------
// SmokePlumeParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using UWGame.SimSide.Entities;
using UWGame.SimSide;
using UWGame.ClientSide.Renderables;
using System.Threading.Tasks;
using WindowSystem;
using UWGame.Control;
using UWGame.SimSide.Systems;
using System.Diagnostics;
#endregion

namespace UWGame.ClientSide.Particles
{
    /// <summary>
    /// parameterized system that emits smoke, dust, fog, fire etc. from a series of emitters  
    /// easier than to create new classes...
    /// </summary>
    [DebuggerDisplay("{ParticleSystemType.KeyName} {AllEmitters.Count}")]
    public class ParticleSystem
    {
        public ParticleSystemType ParticleSystemType;

        //private Dictionary<Vector2, ParticleEmitter> ActiveEmitters = new Dictionary<Vector2, ParticleEmitter>();


       
        /// <summary>
        /// 
        /// </summary>
     //   private List<ParticleEmitter> ActiveEmitters = new List<ParticleEmitter>();

        /// <summary>
        /// the renderable also has this list of its emitters.
        /// </summary>
       // private Dictionary<RenderableID, List<ParticleEmitter>> ActiveAttachedEmitters = new Dictionary<RenderableID, List<ParticleEmitter>>();



        /// <summary>
        /// all emitters - sorted by expiry timepoint
        /// </summary>
        private SleepyUpdater<ParticleEmitter> AllEmitters = new SleepyUpdater<ParticleEmitter>(Module.Client);
        private List<ParticleEmitter> allEmittersForParticleUpdates = new List<ParticleEmitter>();

          

        // the array of particles used by this system. these are reused, so that calling
        // AddParticles will not cause any allocations.
        // Particle[] particles;

        // the queue of free particles keeps track of particles that are not curently
        // being used by an effect. when a new effect is requested, particles are taken
        // from this queue. when particles are finished they are put onto this queue.
        public Queue<Particle> FreeParticles;

        /// <summary>
        /// returns the number of particles that are available for a new effect.
        /// </summary>
        public int FreeParticleCount
        {
            get { return FreeParticles.Count; }
        }

       
        public ParticleSystem(ParticleSystemType type)
        {
            this.ParticleSystemType = type;
           
        }


        /// <summary>
        /// Set up the constants that will give this particle system its behavior and
        /// properties.
        /// 
        /// TODO: deprecate this...
        /// </summary>
     /*   protected override void InitializeConstants()
        {
            if (string.IsNullOrEmpty(TextureFilename))
            {
                TextureFilename = "smokeWhite"; //"smoke";
            }
           

            minInitialSpeed = 20;
            maxInitialSpeed = 100;

            // we don't want the particles to accelerate at all, aside from what we
            // do in our overriden InitializeParticle.
            minAcceleration = 0;
            maxAcceleration = 0;

            // long lifetime, this can be changed to create thinner or thicker smoke.
            // tweak minNumParticles and maxNumParticles to complement the effect.
            minLifetime = 5.0f;
            maxLifetime = 7.0f;

            minScale = .5f;
            maxScale = 1.0f;

            MinNumParticles = 3; //7;
            MaxNumParticles = 6; //15;

            startColor = new Color (238, 218, 126); //(255, 255, 230); //(144, 80, 55);
            endColor = startColor;

            // rotate slowly, we want a fairly relaxed effect
            minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
            maxRotationSpeed = MathHelper.PiOver4 / 2.0f;

           // spriteBlendMode = SpriteBlendMode.AlphaBlend; // XNA 3
            spriteBlendState = BlendState.AlphaBlend; 

           // DrawOrder = AlphaBlendDrawOrder;
        }*/

       
       


        public void RemoveEmitter(ParticleEmitter emitter)
        {
           /* if (emitter.Parent != null)
            {*/
               /* List<ParticleEmitter> list;
                if (ActiveAttachedEmitters.TryGetValue(emitter.Parent.ID, out list))
                {
                    list.Remove(emitter);
                }*/

                emitter.Parent.RemoveParticleEmitter(emitter);
           // }
           /* else
            {
                ActiveEmitters.Remove(emitter);
            }*/

            AllEmitters.Remove(emitter);
            allEmittersForParticleUpdates.Remove(emitter);
        }
       
        protected Vector2 PickRandomDirection(float? meanDirectionInRadiansToUse, float? maxDirectionDifferenceInRadiansToUse)
        {
           
            float radians;

            if (meanDirectionInRadiansToUse.HasValue)
            {
                radians = meanDirectionInRadiansToUse.Value;

                if (maxDirectionDifferenceInRadiansToUse.HasValue)
                {
                    radians += maxDirectionDifferenceInRadiansToUse.Value - 2f * The.Client.ClientRandomGenerator.RandomBetween(0f, maxDirectionDifferenceInRadiansToUse.Value);
                }
            }
            else
            {
                radians = 0f;
            }

            
          /*  Vector2 direction = Vector2.Zero;
            // from the unit circle, cosine is the x coordinate and sine is the
            // y coordinate. We're negating y because on the screen increasing y moves
            // down the monitor.
            direction.X = (float)Math.Cos(radians);
            direction.Y = -(float)Math.Sin(radians);
            */

            return Common.AngleToVector(radians);
            //return direction;
        }

        /// <summary>
        /// PickRandomDirection is used by InitializeParticles to decide which direction
        /// particles will move. The default implementation is a random vector in a
        /// circular pattern.
        /// </summary>
      /*  protected virtual Vector2 PickRandomDirection()
        {
            float angle = Common.RandomBetween(Globals.Instance.Random, 0, MathHelper.TwoPi);
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }*/

        public void Initialize()
        {
            
            // calculate the total number of particles we will ever need, using the
            // max number of effects and the max number of particles per effect.
            // once these particles are allocated, they will be reused, so that
            // we don't put any pressure on the garbage collector.
            //particles = new Particle[howManyEmitters * MaxNumParticles];
            int noOfParticles = ParticleSystemType.NoOfEmitters * ParticleSystemType.MaxNumParticles;
            FreeParticles = new Queue<Particle>(noOfParticles);
            Particle p;
            for (int i = 0; i < noOfParticles; i++)
            {
                p = new Particle();

                if (ParticleSystemType.Animation != null)
                {
                    p.Animation2D = ParticleSystemType.Animation;
                    p.Player = new Animation2DPlayer();
                }

                FreeParticles.Enqueue(p);
            }

        }


      

        /*
        public void RemoveSmokePlume(Entity entity)
        {
            ActiveAttachedEmitters.Remove(entity);
        }
        */
        /// <summary>
        /// InitializeParticle randomizes some properties for a particle, then
        /// calls initialize on it. It can be overriden by subclasses if they 
        /// want to modify the way particles are created. For example, 
        /// SmokePlumeParticleSystem overrides this function make all particles
        /// accelerate to the right, simulating wind.
        /// </summary>
        /// <param name="p">the particle to initialize</param>
        /// <param name="where">the position on the screen that the particle should be
        /// </param>
        public void InitializeParticle(Particle p, float scale, Vector2 where, float lifetimeFactor, float? emitterMeanDirectionInRadians, float? emitterMaxDirectionDifferenceInRadians)
        {
            float? meanDirectionToUse = emitterMeanDirectionInRadians ?? this.ParticleSystemType.MeanDirectionInRadians;
            float? meanDirectionDifferenceToUse = emitterMaxDirectionDifferenceInRadians ?? this.ParticleSystemType.MaxDirectionDifferenceInRadians;
            

            // first, call PickRandomDirection to figure out which way the particle
            // will be moving. velocity and acceleration's values will come from this.
            Vector2 direction = PickRandomDirection(meanDirectionToUse, meanDirectionDifferenceToUse);

            float scaleFactor = ParticleSystemType.SystemScale * scale;

            // pick some random values for our particle
            float velocity = scaleFactor *
                The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minInitialSpeed, ParticleSystemType.maxInitialSpeed);


            float? lifetime;

            if (ParticleSystemType.ParticlesNeverExpire)
            {
                lifetime = null;
            }
            else
            {
                lifetime = lifetimeFactor *
                   The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minLifetime, ParticleSystemType.maxLifetime);
            }

            float acceleration;

            if (!ParticleSystemType.ParticlesNeverExpire && ParticleSystemType.DecelerateParticlesToZeroBeforeTheyDie)
            {
                // Explosions move outwards,
                // then slow down and stop because of air resistance. Let's change
                // acceleration so that when the particle is at max lifetime, the velocity
                // will be zero.

                // We'll use the equation vt = v0 + (a0 * t). (If you're not familar with
                // this, it's one of the basic kinematics equations for constant
                // acceleration, and basically says:
                // velocity at time t = initial velocity + acceleration * t)
                // We'll solve the equation for a0, using t = p.Lifetime and vt = 0.
                acceleration = -velocity / lifetime.Value;
            }
            else
            {
                acceleration = scaleFactor *
                    The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minAcceleration, ParticleSystemType.maxAcceleration);
            }

          


            float finalScale = scaleFactor *
                The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minScale, ParticleSystemType.maxScale);

            float rotationSpeed =
                The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.minRotationSpeed, ParticleSystemType.maxRotationSpeed);

            float rotation = 0f;
            if (ParticleSystemType.rotateRandomlyAtStart == true)
            {
                rotation = The.Client.ClientRandomGenerator.RandomBetween(0, MathHelper.TwoPi);
            }


            // then initialize it with those random values. initialize will save those,
            // and make sure it is marked as active.
            p.Initialize(
                where, rotation, direction * velocity, acceleration * direction,
                lifetime, finalScale, rotationSpeed);

            if (ParticleSystemType.IsAffectedByWind)
            {
                // simulate wind
                p.Acceleration += The.Client.ClientRandomGenerator.RandomBetween(ParticleSystemType.MinWindAccelerationFactor, ParticleSystemType.MaxWindAccelerationFactor)
                    * The.Sim.PlaySite.PlaySite.Weather.WindSpeed * The.Sim.PlaySite.PlaySite.Weather.WindDirection;
            }

            
        }


        public void AddDust(Entity vehicle, Vector2 worldPosition, float intensity, float scale)
        {
            // only add dust for vehicles on-screen?
            ParticleEmitter dustEmitter = vehicle.Renderable.ParticleEmitters.Find(e => e.System.ParticleSystemType.KeyName == "dust");

            if (dustEmitter == null)
            {
                dustEmitter = AddEmitter(vehicle.Renderable, scale, null, Vector2.Zero);
            }


            dustEmitter.Intensity = intensity;

            // add particles manually (outside Update)
            if (dustEmitter.NextEmissionTimePoint == null 
                || The.Sim.TimepointReached(dustEmitter.NextEmissionTimePoint.Value))
            {
                // add a puff of smoke particles 
                dustEmitter.AddParticles(scale, worldPosition, 0.3f + 1.4f * dustEmitter.Intensity); //d.Position);// where); // 0.3f

                // set next time point:
                dustEmitter.NextEmissionTimePoint = UpdateTimePoints.ComputeTimePointFromInterval(dustEmitter.TimeBetweenEmitting.Value);
            }

        /*    if (d.timeTillPuff < 0)
            {
                // add a puff of smoke particles 
                d.AddParticles(scale, worldPosition, 0.3f + 1.4f * d.Intensity); //d.Position);// where); // 0.3f

                // and then reset the timer.
                d.timeTillPuff = d.TimeBetweenEmitting;
            }*/
        }


      /*  private ParticleEmitter GetNewEmitter(Vector2 worldPosition, Vector2 offset)
        {
            ParticleEmitter plume = new ParticleEmitter(this, worldPosition, offset);
            plume.TimeBetweenEmitting = ParticleSystemType.TimeBetweenEmitting;
            
            return plume;
        }*/

       /* public ParticleEmitter AddEmitter(Vector2 worldPosition, float? emitterScale, float? timeBetweenEmitting, Vector2 offset)
        {
            ParticleEmitter emitter = AddEmitter(worldPosition, offset, emitterScale, timeBetweenEmitting);


            return emitter;           
        }*/

        /// <summary>
        /// if timeBetweenEmission is null, the system type's default will be used
        /// </summary>
        /// <param name="renderable"></param>
        /// <param name="scale"></param>
        /// <param name="timeBetweenEmission"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ParticleEmitter AddEmitter(Renderable renderable, float? scale, float? timeBetweenEmission, Vector2 offset, bool emitParticlesInParentsDirection = false)
        {
            Vector2 location = renderable.Location.Value.ToVector2();

            ParticleEmitter emitter = AddEmitter(renderable, location, offset, scale, timeBetweenEmission, emitParticlesInParentsDirection);
            
         //   emitter.Parent = renderable;
           
            return emitter;
           
        }

       // public ParticleEmitter AddEmitter(Vector2 location, Vector2 offset, float? scale, float? timeBetweenEmission, bool emitParticlesInParentsDirection = false)

        /// <summary>
        /// renderable is optional, location will override it.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="offset"></param>
        /// <param name="scale"></param>
        /// <param name="timeBetweenEmission"></param>
        /// <param name="emitParticlesInParentsDirection"></param>
        /// <param name="renderable"></param>
        /// <returns></returns>
        public ParticleEmitter AddEmitter(Renderable renderable, Vector2 location, Vector2 offset, float? scale, float? timeBetweenEmission, bool emitParticlesInParentsDirection = false)
        {
            ParticleEmitter emitter = new ParticleEmitter(this, renderable, location, offset, timeBetweenEmission ?? this.ParticleSystemType.TimeBetweenEmitting, emitParticlesInParentsDirection); // GetNewEmitter(renderable.Location.Value.ToVector2(), offset);

            if (scale.HasValue)
                emitter.EmitterScale = scale.Value;

            AllEmitters.Add(emitter);
            allEmittersForParticleUpdates.Add(emitter);
            return emitter;
        }

        /// <summary>
        /// TODO: cull emitters in the FOW and shroud
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            // here we update the particle positions etc.
            // must be done each frame.
            // we could replace this parallel processing with sleepy updates. Only when the emitter is in view will the particles update every frame - otherwise just once per second or so
            // in this way, the smoke will still have risen when the emitter comes into view again
            // BUT it would be tricky to quickly change to every frame updates when needed.
            // (we could also let the Draw call interpolate/lag behind the infrequent updates?)
            Parallel.ForEach(allEmittersForParticleUpdates, e => e.UpdateParticlesInParallel(gameTime));

        
            AllEmitters.Update(gameTime);
    
            
            // TODO: replace this with Sleepy updates
          /*  ParticleEmitter emitter;
            for (int i = AllEmitters.Count - 1; i >= 0; i--)
            {
                emitter = AllEmitters[i];

                // emit new particles:
                emitter.Update(gameTime);
            }*/

            // replace this with Sleepy updates
          // UpdateEmittersExpiry();

        }



     /*   private void UpdateEmittersExpiry()
        {
            //SleepyUpdater.UpdateExpiring(AllEmitters, ref allEmittersIsDirty,
           //     r => { The.Client.ParticleManager.RemoveEmitter(r); return true; });


            if (AllEmitters != null)
            {
                ParticleEmitter emitter;
                while (AllEmitters.Count > 0)
                {
                    emitter = AllEmitters[0];
                    if (The.Sim.TotalUnPausedGameTimeInSeconds > emitter.TimeToDisappear.Value)
                    {
                        The.Client.ParticleManager.RemoveEmitter(emitter); // removes expired emitter from the list                       
                    }
                    else // done for now.
                    {
                        return;
                    }

                }

            }
        }*/
    }
}

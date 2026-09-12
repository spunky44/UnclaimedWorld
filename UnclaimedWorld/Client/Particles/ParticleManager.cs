using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Map;
using UWGame.ClientSide.Renderables;
using WindowSystem;
using UWGame.SimSide;
using UWGame.SimSide.Systems;
namespace UWGame.ClientSide.Particles
{
    public class ParticleManager
    {
        
      //  ExplosionSmokeParticleSystem smokeSystem;
       // GeneralParticleEmitterSystem smokePlumeSystem; // smoke emitted from fires etc.
      //  GeneralParticleEmitterSystem dustStormSystem; // permanently emitted dust
      //  GeneralParticleEmitterSystem fogSystem; // drifting fog
        
       // FlamerParticleSystem flamePlumeSystem;


      //  AnimatedFireParticleSystem animatedFireSystem; // camp fires etc.
      //  DustParticleSystem dustSystem; // dust triggered by vehicles and called from AI routines

        /// <summary>
        /// used when constructing emitter renderables
        /// </summary>
        RenderableType emitterType;

        public Dictionary<ParticleSystemType, ParticleSystem> ParticleSystems = new Dictionary<ParticleSystemType, ParticleSystem>();



      //  private Dictionary<Vector2, ParticleEmitter> ActiveDustStormPlumes = new Dictionary<Vector2, ParticleEmitter>();
       
     //   private Dictionary<Vector2, FlamerPlume> ActiveFlamePlumes = new Dictionary<Vector2, FlamerPlume>();

        
      
        public ParticleManager()
        {

            emitterType = new RenderableType() { FadeOutWhenDestroyed = true };


        }


        public void Init()
        {
            ParticleSystem system;
            
           // let's just create a particle system for each type that is defined on startup:
            // we could also create them when needed... not sure if that would create lag though...
            ParticleSystems = new Dictionary<ParticleSystemType, ParticleSystem>(); // to enable Init() to be called again
            foreach (var item in GameData.Instance.AllParticleSystems)
            {
                system = new ParticleSystem(item.Value);
                ParticleSystems.Add(item.Value, system);

                system.Initialize();
            }


           /* Animation2D fireAnimation = new Animation2D(The.Client.FlatSpriteSheet.Texture, 0.1f, true);
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_01")));
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_02")));
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_03")));
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_04")));
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_05")));
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_06")));
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_07")));
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_08")));
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_09")));
            fireAnimation.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("campfire_10")));*/

         
            /*
              minInitialSpeed = 0;
            maxInitialSpeed = 0;

            // we don't want the particles to accelerate at all, aside from what we
            // do in our overriden InitializeParticle.
            minAcceleration = 0;
            maxAcceleration = 0;

            // long lifetime, this can be changed to create thinner or thicker smoke.
            // tweak minNumParticles and maxNumParticles to complement the effect.
            minLifetime = 2.0f;
            maxLifetime = 3.0f;

            minScale = .3f;
            maxScale = .5f;

            MinNumParticles = 1; // 5;
            MaxNumParticles = 1; // 10;

          
            minRotationSpeed = -MathHelper.PiOver2 / 2.0f;
            maxRotationSpeed = -MathHelper.Pi;

            // spriteBlendMode = SpriteBlendMode.Additive; // XNA 3
            spriteBlendState = BlendState.Additive;


            
           */


        }

        /*
        public void UpdateExplosions()
        {
            timeTillExplosion -= (float)The.Sim.GameTime.ElapsedGameTime.TotalSeconds;//dt;
            if (timeTillExplosion < 0)
            {
                Vector2 where = Vector2.Zero;
                // create the explosion at some random point on the screen.
                where.X = Common.RandomBetween(Globals.Instance.Random, 40, 600); //UWGame.SimSide.Instance.graphics.GraphicsDevice.Viewport.Width);
                where.Y = Common.RandomBetween(Globals.Instance.Random, 100, 800);//UWGame.SimSide.Instance.graphics.GraphicsDevice.Viewport.Height);

                float scale = Common.RandomBetween(Globals.Instance.Random, 0.1f, 0.3f);
                // the overall explosion effect is actually comprised of two particle
                // systems: the fiery bit, and the smoke behind it. add particles to
                // both of those systems.
                explosionSystem.AddParticles(scale, where);
                smokeSystem.AddParticles(scale, where);

                // reset the timer.
                timeTillExplosion = TimeBetweenExplosions;
            }
        }
        */

        // this function is called when we want to demo the smoke plume effect. it
        // updates the timeTillPuff timer, and adds more particles to the plume when
        // necessary.
     /*   public void UpdateDustStorm()
        {
            foreach (KeyValuePair<Vector2, ParticleEmitter> kvp in ActiveDustStormPlumes)
            {
                kvp.Value.timeTillPuff -= (float)The.Sim.GameTime.ElapsedGameTime.TotalSeconds;
                if (kvp.Value.timeTillPuff < 0)
                {
                    // add more particles at the bottom of the screen, halfway across.
                    //   where.X = graphics.GraphicsDevice.Viewport.Width / 2;
                    //   where.Y = graphics.GraphicsDevice.Viewport.Height;
                    dustStormSystem.AddParticles(kvp.Value.Scale * 0.3f, kvp.Value.Position);// where);

                    // and then reset the timer.
                    kvp.Value.timeTillPuff = kvp.Value.TimeBetweenEmitting; //SmokePlume.TimeBetweenSmokePlumePuffs;
                }
            }

            dustStormSystem.Update(The.Sim.GameTime);
        }*/


        /// <summary>
        /// places a new renderable with an emitter attached on the specified location. Offset should be small...
        /// Emitters never exist without a renderable!
        /// </summary>
        /// <param name="systemKey"></param>
        /// <param name="worldPosition"></param>
        /// <param name="scale"></param>
        /// <param name="timeBetweenPuffs"></param>
        public void AddEmitter(string systemKey, Vector2 worldPosition, float? scale = null, float? timeBetweenPuffs = null, double? durationInSeconds = null, Vector2? offset = null) //, float intensity, float scale)
        {
            // create a renderable to hold the emitter:
            Renderable renderable = RenderableFactory.Produce(null, emitterType, null, durationInSeconds);
            // place the renderable:
            renderable.Location = worldPosition.ToVector3();

            // create an emitter:
            ParticleSystem system = ParticleSystems[GameData.Instance.AllParticleSystems[systemKey]];
            ParticleEmitter emitter = system.AddEmitter(renderable, worldPosition, offset ?? Vector2.Zero, scale, timeBetweenPuffs);
            
          
            renderable.AddParticleEmitter(emitter);

          
            Point mapPos = MapManager.WorldPosToTile(worldPosition);
            The.Map.GetTile(mapPos).AddRenderable(renderable);

        }

        /// <summary>
        /// adds an emitter and attaches it to the renderable
        /// </summary>
        /// <param name="systemKey"></param>
        /// <param name="renderable"></param>
        /// <param name="scale"></param>
        /// <param name="timeBetweenPuffs"></param>
        /// <param name="emitParticlesInParentsDirection"></param>
        public void AddEmitter(string systemKey, Renderable renderable, float? scale = null, float? timeBetweenPuffs = null, bool emitParticlesInParentsDirection = false, double? lifetimeInSeconds = null, Vector2? offset = null) //, float intensity, float scale)
        {
            ParticleSystem system = ParticleSystems[GameData.Instance.AllParticleSystems[systemKey]];

            ParticleEmitter emitter = system.AddEmitter(renderable, scale, timeBetweenPuffs, offset ?? Vector2.Zero, emitParticlesInParentsDirection);
           // emitter.EmitInRenderablesFacingDirection = emitParticlesInParentsDirection;

            if (lifetimeInSeconds.HasValue)
            { 
                // set the time of death for the emitter:
                emitter.ExpiryTimePointInSeconds = UpdateTimePoints.ComputeTimePointFromInterval(lifetimeInSeconds.Value);

                // expiresAtTimepointInTicks = TimeSpan.FromSeconds(DurationInSeconds.Value).Ticks + The.Sim.TotalUnPausedGameTime.Ticks;
            }

           
            // attach the emitter to the renderable:
            renderable.AddParticleEmitter(emitter);

        }

        public void RemoveEmitter(ParticleEmitter emitter) // string systemKey, Renderable renderable, float? scale = null, float? timeBetweenPuffs = null) //, float intensity, float scale)
        {
            ParticleSystem system = ParticleSystems[emitter.System.ParticleSystemType];

            system.RemoveEmitter(emitter);

        }

     /*   public ParticleEmitter AddFog(Vector2 worldPosition, float? scale = null, float? timeBetweenPuffs = null) //, float intensity, float scale)
        {
            return fogSystem.AddEmitter(worldPosition, scale, timeBetweenPuffs);
        }*/

       /* public void AddSmokePlume(Point tilePos, Vector2 offset, float? scale, float? timeBetweenPuffs)
        {
            AddSmokePlume(MapManager.TileToWorldPosVector2(tilePos) + offset, scale, timeBetweenPuffs);
        }*/

      /*  public ParticleEmitter AddSmokePlume(Vector2 worldPosition, float? scale, float? timeBetweenPuffs)
        {
            return smokePlumeSystem.AddEmitter(worldPosition, scale, timeBetweenPuffs);
            
        }*/

      /*  public void RemoveSmokePlume(Entity entity)
        {
            //ActiveSmokePlumes
            //animatedFireSystem.RemoveFire(entity);

            smokePlumeSystem.RemoveSmokePlume(entity);

        }*/

    /*    public void AddDustStormPlume(Point tilePos, Vector2 offset, float? scale, float? timeBetweenPuffs)
        {
            AddDustStormPlume(MapManager.TileToWorldPosVector2(tilePos) + offset, scale, timeBetweenPuffs);
        }

        public void AddDustStormPlume(Vector2 worldPosition, float? scale, float? timeBetweenPuffs)
        {
            if (!ActiveDustStormPlumes.ContainsKey(worldPosition))
            {
                ParticleEmitter plume = new ParticleEmitter();
                plume.Position = worldPosition;

                if (scale.HasValue)
                    plume.Scale = scale.Value;

                if (timeBetweenPuffs.HasValue)
                    plume.TimeBetweenEmitting = timeBetweenPuffs.Value;

                ActiveDustStormPlumes.Add(worldPosition, plume);
            }
        }*/

        
        /*
        public void AddFlamePlume(Vector2 worldPosition)
        {
            if (!ActiveFlamePlumes.ContainsKey(worldPosition))
            {
                FlamerPlume plume = new FlamerPlume();
                plume.Position = worldPosition;
                ActiveFlamePlumes.Add(worldPosition, plume);
            }
        }
        */
        /*
        public FireAndSmoke AddFireAndSmoke(Entity entity, float intensity, float scale)
        {
            FirePlume fire = animatedFireSystem.AddFire(entity, entity.Location, intensity, scale);
            ParticleEmitter smoke = AddEmitter("smoke", entity.Location.ToVector2(), scale, null);
            LightSource newLightSource = new LightSource(entity.Location);

            FireAndSmoke fireAndSmoke = new FireAndSmoke() { FirePlume = fire, SmokePlume = smoke, LightSource = newLightSource };

            
            return fireAndSmoke;
        }*/

        /*
        public void AddFire(Entity entity, Vector3 worldPosition, float intensity, float scale)
        {
            if (The.MapUI.WorldPositionIsOnScreen(worldPosition, 10f))
            {
                animatedFireSystem.AddFire(entity, worldPosition, intensity, scale);
            }
        }
        */

       /* public void RemoveFire(Entity entity)
        {
            animatedFireSystem.RemoveFire(entity);
            
        }

        public FirePlume GetFirePlume(Entity entity)
        {
            return animatedFireSystem.GetFirePlume(entity);
        }*/

        public void AddDust(Entity vehicle,  Vector3 worldPosition, float intensity, float scale)
        {
            if (The.MapUI.WorldPositionIsOnScreen(worldPosition, 10f))
            {
                ParticleSystem system = ParticleSystems[GameData.Instance.AllParticleSystems["dust"]];
                system.AddDust(vehicle, new Vector2(worldPosition.X, worldPosition.Y), intensity, scale);
            }

          /*  if (!ActiveFlamePlumes.ContainsKey(worldPosition))
            {
                FlamerPlume plume = new FlamerPlume();
                plume.Position = worldPosition;
                ActiveFlamePlumes.Add(worldPosition, plume);
            }*/
        }

       

       /// <summary>
       /// Where in world coords. scale should be 0.1f - 0.5f or so...
       /// </summary>
       /// <param name="scale"></param>
       /// <param name="where"></param>
     /*   public void CreateExplosion(float scale, Vector2 where)
        {
                //Vector2 where = Vector2.Zero;
                // create the explosion at some random point on the screen.
                //where.X = RandomBetween(0, graphics.GraphicsDevice.Viewport.Width);
                //where.Y = RandomBetween(0, graphics.GraphicsDevice.Viewport.Height);

                // the overall explosion effect is actually comprised of two particle
                // systems: the fiery bit, and the smoke behind it. add particles to
                // both of those systems.


            ParticleSystems[GameData.Instance.AllParticleSystems["explosion"]].AddEmitter(where, Vector2.Zero, scale, null);
            ParticleSystems[GameData.Instance.AllParticleSystems["explosionSmokeCloud"]].AddEmitter(where, Vector2.Zero, scale, null);
            
        }*/

        public void Update()
        {
        
            // each system updates its own emitters
            foreach (var item in ParticleSystems)
            {
                item.Value.Update(The.Sim.GameTime);
            }
        }

       

        /*
        public void UpdateFlamePlumes()
        {

            foreach (KeyValuePair<Vector2, FlamerPlume> kvp in ActiveFlamePlumes)
            {
                kvp.Value.timeTillFlamerPuff -= (float)The.Sim.GameTime.ElapsedGameTime.TotalSeconds;
                if (kvp.Value.timeTillFlamerPuff < 0)
                {
                    // add more particles at the bottom of the screen, halfway across.
                    //   where.X = graphics.GraphicsDevice.Viewport.Width / 2;
                    //   where.Y = graphics.GraphicsDevice.Viewport.Height;
                    flamePlumeSystem.AddParticles(0.3f, kvp.Value.Position);// where);

                    // and then reset the timer.
                    kvp.Value.timeTillFlamerPuff = FlamerPlume.TimeBetweenFlamerPuffs;
                }
            }
             
        }*/
    }
}

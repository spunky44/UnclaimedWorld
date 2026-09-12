using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using WindowSystem;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace UWGame.ClientSide.Particles
{
    [DebuggerDisplay("{KeyName}")]
    public class ParticleSystemType : IGameData
    {


        /// <summary>  
        /// the texture this particle system will use.
        /// TODO: change to use (billboard?) sprite sheets - since we are rendering together with billboards
        /// </summary>
        [XmlIgnore]
        public Texture2D Texture;

        // the origin when we're drawing textures. this will be the middle of the
        // texture.
        public Vector2 Origin;

        // these two values control the order that particle systems are drawn in.
        // typically, particles that use additive blending should be drawn on top of
        // particles that use regular alpha blending. ParticleSystems should therefore
        // set their DrawOrder to the appropriate value in InitializeConstants, though
        // it is possible to use other values for more advanced effects.
        public const int AlphaBlendDrawOrder = 100;
        public const int AdditiveDrawOrder = 200;


        /// <summary>
        /// Scale of sprite size, accel and velocity.
        /// gets mulitplied with emitter scale, and with the random particle scale (minScale + maxScale)
        /// </summary>
        public float SystemScale = 1f;

        /// <summary>
        /// used in explosions
        /// </summary>
        public bool DecelerateParticlesToZeroBeforeTheyDie = false;


        // this number represents the maximum number of effects this particle system
        // will be expected to draw at one time. this is set in the constructor and is
        // used to calculate how many particles we will need.
        public int NoOfEmitters {get; private set; }

        // public float TimeBetweenEmitting = 2f;


        #region Mode - continual emission (smoke plume), one-off (explosion, smoke cloud) or code driven (dust) emission is set here

        /// <summary>
        /// a number here means continual emitting
        /// </summary>
        public float? TimeBetweenEmitting = null;
      
     
        /// <summary>
        /// only set this true for special systems like the vehicle dust that creates particles from code systems.
        /// </summary>
        public bool EmitParticlesManually = false;

        /// <summary>
        /// use this for explosions and other short lived effects
        /// </summary>
        public bool RemoveEmittersAfterLastParticleExpires = false;

        /// <summary>
        /// Set this true for flames and such that continue animating for the emitter's lifetime
        /// </summary>
        public bool ParticlesNeverExpire = false;


        #endregion


        public bool IsAffectedByWind = true;


        public float MinWindAccelerationFactor = 0.5f;
        public float MaxWindAccelerationFactor = 1f;

        public bool rotateRandomlyAtStart = true;

        /// <summary>
        /// minNumParticles and maxNumParticles control the number of particles that are
        /// added when AddParticles is called. The number of particles will be a random
        /// number between minNumParticles and maxNumParticles.
        /// </summary>
        public int MinNumParticles;
        /// <summary>
        /// minNumParticles and maxNumParticles control the number of particles that are
        /// added when AddParticles is called. The number of particles will be a random
        /// number between minNumParticles and maxNumParticles.
        /// </summary>
        public int MaxNumParticles;

        public Color StartColor = Color.White; // Vector3.One;
        public Color endColor = Color.White;

        public string AnimationKey;

        [XmlIgnore]
        public Animation2D Animation
        {
            get;
            private set;
        }

        /// <summary>
        /// this controls the texture that the particle system uses. It will be used as
        /// an argument to ContentManager.Load.
        /// </summary>
        public string TextureFilename;

        /// <summary>
        /// minInitialSpeed and maxInitialSpeed are used to control the initial velocity
        /// of the particles. The particle's initial speed will be a random number 
        /// between these two. The direction is determined by the function 
        /// PickRandomDirection, which can be overriden.
        /// </summary>
        public float minInitialSpeed;
        /// <summary>
        /// minInitialSpeed and maxInitialSpeed are used to control the initial velocity
        /// of the particles. The particle's initial speed will be a random number 
        /// between these two. The direction is determined by the function 
        /// PickRandomDirection, which can be overriden.
        /// </summary>
        public float maxInitialSpeed;

        /// <summary>
        /// minAcceleration and maxAcceleration are used to control the acceleration of
        /// the particles. The particle's acceleration will be a random number between
        /// these two. By default, the direction of acceleration is the same as the
        /// direction of the initial velocity.
        /// </summary>
        public float minAcceleration;
        /// <summary>
        /// minAcceleration and maxAcceleration are used to control the acceleration of
        /// the particles. The particle's acceleration will be a random number between
        /// these two. By default, the direction of acceleration is the same as the
        /// direction of the initial velocity.
        /// </summary>
        public float maxAcceleration;

        /// <summary>
        /// minRotationSpeed and maxRotationSpeed control the particles' angular
        /// velocity: the speed at which particles will rotate. Each particle's rotation
        /// speed will be a random number between minRotationSpeed and maxRotationSpeed.
        /// Use smaller numbers to make particle systems look calm and wispy, and large 
        /// numbers for more violent effects.
        /// </summary>
        public float minRotationSpeed;
        /// <summary>
        /// minRotationSpeed and maxRotationSpeed control the particles' angular
        /// velocity: the speed at which particles will rotate. Each particle's rotation
        /// speed will be a random number between minRotationSpeed and maxRotationSpeed.
        /// Use smaller numbers to make particle systems look calm and wispy, and large 
        /// numbers for more violent effects.
        /// </summary>
        public float maxRotationSpeed;

        /// <summary>
        /// minLifetime and maxLifetime are used to control the lifetime. Each
        /// particle's lifetime will be a random number between these two. Lifetime
        /// is used to determine how long a particle "lasts." Also, in the base
        /// implementation of Draw, lifetime is also used to calculate alpha and scale
        /// values to avoid particles suddenly "popping" into view
        /// </summary>
        public float minLifetime;
        /// <summary>
        /// minLifetime and maxLifetime are used to control the lifetime. Each
        /// particle's lifetime will be a random number between these two. Lifetime
        /// is used to determine how long a particle "lasts." Also, in the base
        /// implementation of Draw, lifetime is also used to calculate alpha and scale
        /// values to avoid particles suddenly "popping" into view
        /// </summary>
        public float maxLifetime;

        /// <summary>
        /// to get some additional variance in the appearance of the particles, we give
        /// them all random scales. the scale is a value between minScale and maxScale,
        /// and is additionally affected by the particle's lifetime to avoid particles
        /// "popping" into view.
        /// </summary>
        public float minScale;
        /// <summary>
        /// to get some additional variance in the appearance of the particles, we give
        /// them all random scales. the scale is a value between minScale and maxScale,
        /// and is additionally affected by the particle's lifetime to avoid particles
        /// "popping" into view.
        /// </summary>
        public float maxScale;



        /// <summary>
        /// different effects can use different blend modes. fire and explosions work
        /// well with additive blending, for example.
        /// </summary>
        public BlendState spriteBlendState;
        //protected SpriteBlendMode spriteBlendMode; // XNA 3

      

        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }
        [XmlIgnore]
        public float? MeanDirectionInRadians
        {
            get;
            private set;
        }

        [XmlIgnore]
        public float? MaxDirectionDifferenceInRadians
        {
            get;
            private set;
        }


        public float StartMeanMoveDirectionInDegrees; //= 80;
       


        /// <summary>
        /// how far to each side of the mean the direction can differ
        /// </summary>
        public float StartMaxMoveDirectionDifferenceInDegrees; //= 80;
       


        public ParticleSystemType()
        {

        }

        public ParticleSystemType(string key, int howManyEmitters)
        {
            this.KeyName = key;

            this.NoOfEmitters = howManyEmitters;

        }

        public void LoadContent(ContentManager content)
        {

            if (TextureFilename != null)
            {
                // load the texture....
                Texture = content.Load<Texture2D>(TextureFilename);

                // ... and calculate the center. this'll be used in the draw call, we
                // always want to rotate and scale around this point.
                Origin.X = Texture.Width / 2;
                Origin.Y = Texture.Height / 2;
            }

            //  base.LoadContent();
        }

        public void PreInitValidate(ref List<string> errors)
        {
           
        }

         /// <summary>
        /// some init depend on the sprite maps/ utility maps having been loaded. put the code here:
        /// </summary>
        /// <param name="parent"></param>
        public void PostLoadContentInitialize()
        {
            if (!string.IsNullOrEmpty(AnimationKey))
            {
                Animation = GameData.Instance.Animation2Ds[AnimationKey]; 
                
            }
        }

        public void Initialize()
        {
            MeanDirectionInRadians = MathHelper.ToRadians(StartMeanMoveDirectionInDegrees);

            MaxDirectionDifferenceInRadians = MathHelper.ToRadians(StartMaxMoveDirectionDifferenceInDegrees);
        }

        public void PostInitValidate(ref List<string> errors)
        {
           
        }

        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}

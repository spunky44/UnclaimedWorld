using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.ClientSide;
using UWGame.ClientSide.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.SimSide.AllGameData
{
    public class ParticlesLoader
    {


        public static List<ParticleSystemType> Init()
        {

            List<ParticleSystemType> list = new List<ParticleSystemType>();

             
                  ParticleSystemType system;
                  system = new ParticleSystemType("smallFire", 10);  //dunno what all this data is, the only thing that this is for is making the flame PNG anim at the bottom of this list....the smokeplume is then added in GameDataLoader at "lightFire"  MP oct2013
                  system.minInitialSpeed = 0;
                  system.maxInitialSpeed = 0;
                  system.minAcceleration = 0;
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 0;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.TextureFilename = "explosion";    //remove this           
                  system.IsAffectedByWind = false;
                  system.rotateRandomlyAtStart = false;
                  system.minRotationSpeed = 0;
                  system.maxRotationSpeed = 0;
                  system.minScale = 1;
                  system.maxScale = 1f;
                  system.MinNumParticles = 1;
                  system.MaxNumParticles = 1;
                  system.ParticlesNeverExpire = true;
                  //system.minLifetime = 100f; //  
                  //system.maxLifetime = 100f;          
                  system.spriteBlendState = BlendState.Additive;
                  system.AnimationKey = "campfireSmall"; //PNG anim defined in GameData.cs WHY THE HECK does the anim wiggle so much?? that's not how I animated it. //was campfireFast oct 2013. (that is a bigger bonfire).. changed to campfireSmall for smaller flames
                //  system.Initialize();
                  list.Add(system);

                  system = new ParticleSystemType("tinyFire", 10);
                  system.minInitialSpeed = 1;
                  system.maxInitialSpeed = 3;
                  system.minAcceleration = 0;
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 270;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 3;
                  system.TextureFilename = "explosion";
                  system.MaxWindAccelerationFactor = 0.5f;
                  system.MinWindAccelerationFactor = 1f;
                  system.IsAffectedByWind = true;
                  system.minRotationSpeed = -MathHelper.PiOver2 / 2.0f;
                  system.maxRotationSpeed = -MathHelper.Pi;
                  system.minScale = 0.005f;
                  system.maxScale = 0.02f;
                  system.MinNumParticles = 10;
                  system.MaxNumParticles = 15;
                  system.minLifetime = 1f;
                  system.maxLifetime = 2f;
                  system.spriteBlendState = BlendState.Additive;
                  system.TimeBetweenEmitting = 0.1f;
                 // system.Initialize();
                  list.Add(system);

                  system = new ParticleSystemType("bonfire", 10);
                  system.minInitialSpeed = 0;
                  system.maxInitialSpeed = 0;
                  system.minAcceleration = 0;
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 0;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.TextureFilename = "explosion";    //remove this           
                  system.IsAffectedByWind = false;
                  system.rotateRandomlyAtStart = false;
                  system.minRotationSpeed = 0;
                  system.maxRotationSpeed = 0;
                  system.minScale = 1;
                  system.maxScale = 1f;
                  system.MinNumParticles = 1;
                  system.MaxNumParticles = 1;
                  system.ParticlesNeverExpire = true;
                  //system.minLifetime = 100f; //  
                  //system.maxLifetime = 100f;          
                  system.spriteBlendState = BlendState.Additive;
                  system.AnimationKey = "campfireFast"; //PNG anim defined in GameData.cs 
                 // system.Initialize();
                  list.Add(system);


                  system = new ParticleSystemType("explosion", 2);
                  system.SystemScale = 0.3f;
                  system.minInitialSpeed = 40;   // high initial speed with lots of variance.  make the values closer
                  system.maxInitialSpeed = 500; // together to have more consistently circular explosions.
                  system.minAcceleration = 0; // we use deceleration instead
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "explosion";
                  system.RemoveEmittersAfterLastParticleExpires = true; // one explosion per emitter           
                  system.IsAffectedByWind = false;
                  system.DecelerateParticlesToZeroBeforeTheyDie = true;
                  system.minRotationSpeed = -MathHelper.PiOver4;
                  system.maxRotationSpeed = MathHelper.PiOver4;
                  system.minScale = 0.3f;
                  system.maxScale = 1f;
                  system.MinNumParticles = 5;
                  system.MaxNumParticles = 10;
                  system.minLifetime = 0.5f;  // explosions should be relatively short lived
                  system.maxLifetime = 1f;
                  system.spriteBlendState = BlendState.Additive;
                  //system.Initialize(); // delete this??
                  list.Add(system);



                  // the smoke from the explosion lingers a while.       
                  system = new ParticleSystemType("explosionSmokeCloud", 8);
                  system.SystemScale = 0.3f;
                  system.minInitialSpeed = 20;     // less initial speed than the explosion itself
                  system.maxInitialSpeed = 200;
                  system.minAcceleration = -10;  // acceleration is negative, so particles will accelerate away from the
                  system.maxAcceleration = -50; // initial velocity.  this will make them slow down, as if from wind
                  // resistance. we want the smoke to linger a bit and feel wispy, though,
                  // so we don't stop them completely like we do ExplosionParticleSystem
                  // particles.         
                  system.TextureFilename = "smoke";
                  system.RemoveEmittersAfterLastParticleExpires = true; // one explosion per emitter            
                  system.IsAffectedByWind = true;
                  system.minRotationSpeed = -MathHelper.PiOver4;
                  system.maxRotationSpeed = MathHelper.PiOver4;
                  system.minScale = 1f;
                  system.maxScale = 2f;
                  system.MinNumParticles = 10;
                  system.MaxNumParticles = 20;
                  system.minLifetime = 1f;  // explosion smoke lasts for longer than the explosion itself, but not
                  system.maxLifetime = 2.5f;    // as long as the plumes do.            
                  system.spriteBlendState = BlendState.AlphaBlend;
                 // system.Initialize();
                  list.Add(system);



                  system = new ParticleSystemType("dustStorm", 180);
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "smoke";
                  system.StartColor = new Color(247, 226, 181);//(196, 122, 96)
                  system.endColor = system.StartColor;
                  system.minInitialSpeed = 10;
                  system.maxInitialSpeed = 30;
                  system.minRotationSpeed = -MathHelper.PiOver2 / 10.0f;
                  system.maxRotationSpeed = MathHelper.PiOver2 / 10.0f;
                  system.minLifetime = 4.0f;
                  system.maxLifetime = 8.0f;
                //  system.Initialize();
                  list.Add(system);

                  //    UWGame.SimSide.Instance.Components.Add(smokePlumeSystem);


                  //flamePlumeSystem = new FlamerParticleSystem(9);
                  //  flamePlumeSystem.Initialize();
                  //    UWGame.SimSide.Instance.Components.Add(flamePlumeSystem);

                  //dustSystem = new DustParticleSystem(9, 30);
                  // dustSystem.Initialize();



                  /*
                TextureFilename = "explosion";

               minInitialSpeed = 240;
               maxInitialSpeed = 300;

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

               MinNumParticles = 5;
               MaxNumParticles = 10;

          
               minRotationSpeed = -MathHelper.PiOver2 / 2.0f;
               maxRotationSpeed = -MathHelper.Pi;

               // spriteBlendMode = SpriteBlendMode.Additive; // XNA 3
               spriteBlendState = BlendState.Additive; 
                */

                  // useful for flamers and oil/gas fires
                  system = new ParticleSystemType("flamePlume", 40); //9);
                  system.minInitialSpeed = 240;
                  system.maxInitialSpeed = 300;
                  system.minAcceleration = 0;
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "explosion";
                  system.MaxWindAccelerationFactor = 0.5f;
                  system.MinWindAccelerationFactor = 1f;
                  system.IsAffectedByWind = true;
                  system.minRotationSpeed = -MathHelper.PiOver2 / 2.0f;
                  system.maxRotationSpeed = -MathHelper.Pi;
                  system.minScale = 0.3f;
                  system.maxScale = 0.5f;
                  system.MinNumParticles = 5;
                  system.MaxNumParticles = 10;
                  system.minLifetime = 2f;
                  system.maxLifetime = 3f;
                  system.spriteBlendState = BlendState.Additive;
                  system.TimeBetweenEmitting = 0.1f; 
               //   system.Initialize();
                  list.Add(system);

                  system = new ParticleSystemType("fireExtinguisherPoison", 20); //9); //this particle anim needs to correspond with the weapon's range and width of cone
                  system.minInitialSpeed = 240;
                  system.maxInitialSpeed = 300;
                  system.minAcceleration = -30;
                  system.maxAcceleration = -50;
                  //system.StartMeanMoveDirectionInDegrees = 90; // not used with attached/entity facing direction
                  system.StartMaxMoveDirectionDifferenceInDegrees = 26; //10   gives variance in start direction
                  system.TextureFilename = "smokeWhite"; //using this texture because system.StartColor has little effect on smoke.bmp (which remains greyish)
                  system.MaxWindAccelerationFactor = 0.5f;
                  system.MinWindAccelerationFactor = 1f;
                  system.IsAffectedByWind = false; // true;
                  system.minRotationSpeed = -MathHelper.PiOver2 / 2.0f;
                  system.maxRotationSpeed = -MathHelper.Pi;
                  system.minScale = 0.05f; //0.0005f  0.1f
                  system.maxScale = 0.15f; //0.2f
                  system.MinNumParticles = 10;
                  system.MaxNumParticles = 15;
                  system.StartColor = new Color(235, 255, 200); //(250, 255, 217)
                  system.endColor = new Color(255, 0, 0); //(194, 255, 122) mp feb 2015: setting end color did not have any effect, only start color was seen.
                  system.minLifetime = 0.5f;
                  system.maxLifetime = 0.7f; //0.7f
                  system.TimeBetweenEmitting = 0.1f; // 5f;
                //  system.Initialize();
                  list.Add(system);


                  system = new ParticleSystemType("bushDragonSpray", 20); //9); //this particle anim needs to correspond with the weapon's range and width of cone
                  system.minInitialSpeed = 100;
                  system.maxInitialSpeed = 140;
                  system.minAcceleration = -40;
                  system.maxAcceleration = -50;
                  //system.StartMeanMoveDirectionInDegrees = 90; // not used with attached/entity facing direction
                  system.StartMaxMoveDirectionDifferenceInDegrees = 26; //10   gives variance in start direction
                  system.TextureFilename = "smokeWhite"; //using this texture because system.StartColor has little effect on smoke.bmp (which remains greyish)
                  system.MaxWindAccelerationFactor = 0.5f;
                  system.MinWindAccelerationFactor = 1f;
                  system.IsAffectedByWind = false; // true;
                  system.minRotationSpeed = -MathHelper.PiOver2 / 2.0f;
                  system.maxRotationSpeed = -MathHelper.Pi;
                  system.minScale = 0.05f; //0.0005f  0.1f
                  system.maxScale = 0.15f; //0.2f
                  system.MinNumParticles = 10;
                  system.MaxNumParticles = 15;
                  system.StartColor = new Color(235, 255, 200); //(250, 255, 217)
                  system.endColor = new Color(255, 0, 0); //(194, 255, 122) mp feb 2015: setting end color did not have any effect, only start color was seen.
                  system.minLifetime = 0.5f;
                  system.maxLifetime = 0.7f; //0.7f
                  system.TimeBetweenEmitting = 0.3f; //0.1f
                //  system.Initialize();
                  list.Add(system);



                  system = new ParticleSystemType("dust", 40); //9);
                  system.minInitialSpeed = 0;
                  system.maxInitialSpeed = 20;
                  system.minAcceleration = 0;
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "smoke";
                  system.EmitParticlesManually = true;
                  system.MaxWindAccelerationFactor = 0.5f;
                  system.MinWindAccelerationFactor = 1f;
                  system.IsAffectedByWind = true;
                  system.minRotationSpeed = -MathHelper.PiOver2 / 10.0f;
                  system.maxRotationSpeed = MathHelper.PiOver2 / 10.0f;
                  system.minScale = 1f;
                  system.maxScale = 3f;
                  system.MinNumParticles = 2;
                  system.MaxNumParticles = 3;
                  system.StartColor = new Color(196, 122, 96);
                  system.endColor = system.StartColor;
                  system.minLifetime = 4.0f;
                  system.maxLifetime = 6.0f;
               //   system.Initialize();
                  list.Add(system);



                  //yellow smoke from sulphurous lakes
                  system = new ParticleSystemType("sulphurousSmoke", 40); //9);
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.minInitialSpeed = 40;
                  system.maxInitialSpeed = 60;
                  system.TextureFilename = "smoke";
                  system.SystemScale = 0.3f;
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minScale = .5f;
                  system.maxScale = 1.0f;
                  system.MinNumParticles = 3; //7;
                  system.MaxNumParticles = 6; //15;
                  system.StartColor = new Color(238, 218, 126); //(255, 255, 230); //(144, 80, 55);
                  system.endColor = system.StartColor;
                  system.minLifetime = 5.0f;
                  system.maxLifetime = 7.0f;
                  system.TimeBetweenEmitting = 0.5f;
               //   system.Initialize();
                  list.Add(system);

                  system = new ParticleSystemType("sulfurBomb", 4); //9);
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.minInitialSpeed = 40;
                  system.maxInitialSpeed = 60;
                  system.TextureFilename = "smoke";
                  system.SystemScale = 0.3f; //mp april 2015 was 0.2f
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minScale = .5f;
                  system.maxScale = 1.0f;
                  system.MinNumParticles = 3; //7;
                  system.MaxNumParticles = 6; //15;
                  system.StartColor = new Color(238, 218, 126); //(255, 255, 230); //(144, 80, 55);
                  system.endColor = system.StartColor;
                  system.minLifetime = 6.0f;//mp april 2015 was 4.0f
                  system.maxLifetime = 8.0f;//mp april 2015 was 6.0f
               //   system.Initialize();
                  list.Add(system);

                  //smoke rising from campfire
                  system = new ParticleSystemType("smallSmoke", 40); //9);
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.minInitialSpeed = 40;
                  system.maxInitialSpeed = 60;
                  system.TextureFilename = "smoke";
                  system.SystemScale = 0.3f;
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minScale = .5f;
                  system.maxScale = 1.0f;
                  system.MinNumParticles = 3; //7;
                  system.MaxNumParticles = 6; //15;
                  system.StartColor = new Color(255, 255, 255); //(255, 255, 230); //(144, 80, 55);
                  system.endColor = system.StartColor;
                  system.minLifetime = 5.0f;
                  system.maxLifetime = 7.0f;
               //   system.Initialize();
                  list.Add(system);


                  //smaller campfire smoke
                  system = new ParticleSystemType("smallerSmoke", 8); //11); 
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0.8f;
                  system.minInitialSpeed = 40;
                  system.maxInitialSpeed = 60;
                  system.TextureFilename = "smoke";
                  system.SystemScale = 0.15f;
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minScale = .3f;  //start size...? 0.5
                  system.maxScale = 1.5f;  //end size I presume...MP  1.0
                  system.MinNumParticles = 3; //7;
                  system.MaxNumParticles = 6; //15;
                  system.StartColor = new Color(255, 255, 255); //(255, 255, 230); //(144, 80, 55);
                  system.endColor = system.StartColor;
                  system.minLifetime = 10f;
                  system.maxLifetime = 20f;
                  system.TimeBetweenEmitting = 0.5f;
              //    system.Initialize();
                  list.Add(system);

                  // smallest smoke - not 50 m high smoke columns
                  system = new ParticleSystemType("smallestSmoke", 8); //11); 
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0.8f;
                  system.minInitialSpeed = 40;
                  system.maxInitialSpeed = 60;
                  system.TextureFilename = "smoke";
                  system.SystemScale = 0.15f;
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minScale = .2f;  //start size...? 0.5
                  system.maxScale = 1.2f;  //end size I presume...MP  1.0 // Lars: no, read the comment: 
                  system.MinNumParticles = 2; //7;
                  system.MaxNumParticles = 4; //15;
                  system.StartColor = new Color(255, 255, 255); //(255, 255, 230); //(144, 80, 55);
                  system.endColor = system.StartColor;
                  system.minLifetime = 4f;
                  system.maxLifetime = 10f;
                  system.TimeBetweenEmitting = 0.5f;
              //    system.Initialize();
                  list.Add(system);



//food steam from field kitchen (uses gas)
                  system = new ParticleSystemType("foodSteam", 4); 
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0.8f;
                  system.minInitialSpeed = 40;
                  system.maxInitialSpeed = 60;
                  system.TextureFilename = "smoke";
                  system.SystemScale = 0.05f;
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minScale = 0.2f;  //not start size
                  system.maxScale = 1.2f;  //not end size
                  system.MinNumParticles = 2; //7;
                  system.MaxNumParticles = 4; //15;
                  system.StartColor = new Color(255, 255, 255); //mp: how do I make it white? is grey. whiteSignalSmoke underneath has same color defined?
                  system.endColor = system.StartColor;
                  system.minLifetime = 2f;
                  system.maxLifetime = 4f;
                  system.TimeBetweenEmitting = 0.5f;
              //    system.Initialize();
                  list.Add(system);


                  //black smoke column
                  system = new ParticleSystemType("signalSmoke", 40); //9);
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0.8f;
                  system.minInitialSpeed = 40;
                  system.maxInitialSpeed = 60;
                  system.TextureFilename = "smoke";
                  system.SystemScale = 0.15f;
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minScale = .5f;
                  system.maxScale = 1.0f;
                  system.MinNumParticles = 3; //7;
                  system.MaxNumParticles = 6; //15;
                  system.StartColor = new Color(0, 0, 0); //(255, 255, 230); //(144, 80, 55);
                  system.endColor = system.StartColor;
                  system.minLifetime = 10f;
                  system.maxLifetime = 20f;
                  system.TimeBetweenEmitting = 0.5f;
              //    system.Initialize();
                  list.Add(system);

            //white smoke column
                  system = new ParticleSystemType("whiteSignalSmoke", 40); //9);
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0.8f;
                  system.minInitialSpeed = 40;
                  system.maxInitialSpeed = 60;
                  system.TextureFilename = "smoke";
                  system.SystemScale = 0.15f;
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minScale = .5f;
                  system.maxScale = 1.0f;
                  system.MinNumParticles = 3; //7;
                  system.MaxNumParticles = 6; //15;
                  system.StartColor = new Color(255, 255, 255); //yellow-grey: (218, 214, 183)
                  system.endColor = new Color(255, 255, 255); //white
                  system.minLifetime = 10f;
                  system.maxLifetime = 20f;
                  system.TimeBetweenEmitting = 0.5f;
              //    system.Initialize();
                  list.Add(system);



                  //*** Fog system: ***Big mist which rises over wet areas in the early morning, but will get burned away by the sun...therefore can only be present during day if it's cloudy. Ties together groundFog
                  system = new ParticleSystemType("fog", 30);
                  system.TextureFilename = "smoke"; // file name / path            
                  system.SystemScale = 0.3f;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.IsAffectedByWind = true;
                  system.MinWindAccelerationFactor = 0.1f;
                  system.MaxWindAccelerationFactor = 0.1f;
                  system.StartColor = new Color(174, 227, 236); //(218, 249, 255) (109, 134, 139)
                  system.endColor = new Color(174, 227, 236);
                  system.minInitialSpeed = 0;
                  system.maxInitialSpeed = 0;
                  system.minRotationSpeed = 0; // -MathHelper.PiOver2 / 5.0f;
                  system.maxRotationSpeed = 0; // MathHelper.PiOver2 / 10.0f;
                  system.minLifetime = 12.0f;
                  system.maxLifetime = 24.0f;
                  system.TimeBetweenEmitting = 3f;
                  system.MinNumParticles = 0;
                  system.MaxNumParticles = 1;
                  system.minScale = 0.75f;
                  system.maxScale = 2.25f;
             //     system.Initialize();
                  list.Add(system);

                  //Groundfog  laying still on ground, wraps around treetrunks //MP jun 2015: dont use, this looks currently like shit, it's much too dense
                  system = new ParticleSystemType("groundFog", 30);
                  system.TextureFilename = "fog"; // file name / path            
                  system.SystemScale = 0.3f;
                  system.StartMeanMoveDirectionInDegrees = 0;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.IsAffectedByWind = false;
                  system.MinWindAccelerationFactor = 0f;
                  system.MaxWindAccelerationFactor = 0f;
                  system.StartColor = new Color(174, 227, 236);
                  system.endColor = new Color(174, 227, 236);
                  system.minInitialSpeed = 0;
                  system.maxInitialSpeed = 0;
                  system.minRotationSpeed = 0; // -MathHelper.PiOver2 / 5.0f;
                  system.maxRotationSpeed = 0; // MathHelper.PiOver2 / 10.0f;
                  system.rotateRandomlyAtStart = false;
                  system.minLifetime = 12.0f;
                  system.maxLifetime = 22.0f;
                  system.TimeBetweenEmitting = 1f;
                  system.MinNumParticles = 0;
                  system.MaxNumParticles = 1;
                  system.minScale = 1f;
                  system.maxScale = 2f;
                  system.rotateRandomlyAtStart = false;
              //    system.Initialize();
                  list.Add(system);


                  system = new ParticleSystemType("kitchenSteam", 3);
                  system.TextureFilename = "smoke"; // file name / path            
                  system.SystemScale = 0.1f;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.IsAffectedByWind = true;
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.StartColor = new Color(174, 227, 236); // (109, 134, 139)    makes it glow whitish : (218, 249, 255)
                  system.endColor = new Color(174, 227, 236);
                  system.minInitialSpeed = 40;
                  system.maxInitialSpeed = 60;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minLifetime = 2f;
                  system.maxLifetime = 4f;
                  system.TimeBetweenEmitting = 0.5f;
                  system.MinNumParticles = 0;
                  system.MaxNumParticles = 1;
                  system.minScale = 0.75f;
                  system.maxScale = 1.25f;
             //     system.Initialize();
                  list.Add(system);


                  //Little ""steam" clouds rising
                  system = new ParticleSystemType("smallFog", 30);
                  system.TextureFilename = "smoke"; // file name / path            
                  system.SystemScale = 0.3f;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.IsAffectedByWind = true;
                  system.MinWindAccelerationFactor = 0.1f;
                  system.MaxWindAccelerationFactor = 0.1f;
                  system.StartColor = new Color(174, 227, 236); // (109, 134, 139)    makes it glow whitish : (218, 249, 255)
                  system.endColor = new Color(174, 227, 236);
                  system.minInitialSpeed = 0;
                  system.maxInitialSpeed = 0;
                  system.minRotationSpeed = 0; // -MathHelper.PiOver2 / 5.0f;
                  system.maxRotationSpeed = 0; // MathHelper.PiOver2 / 10.0f;
                  system.minLifetime = 12.0f;
                  system.maxLifetime = 24.0f;
                  system.TimeBetweenEmitting = 3f;
                  system.MinNumParticles = 0;
                  system.MaxNumParticles = 1;
                  system.minScale = 0.75f;
                  system.maxScale = 1.25f;
             //     system.Initialize();
                  list.Add(system);



                  //HAZE. concentration of dust/smoke particles hanging in hot areas during the day. is brownish
                  system = new ParticleSystemType("haze", 30);
                  system.TextureFilename = "smoke"; // file name / path            
                  system.SystemScale = 0.3f;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.IsAffectedByWind = true;
                  system.MinWindAccelerationFactor = 0.1f;
                  system.MaxWindAccelerationFactor = 0.1f;
                  system.StartColor = new Color(247, 226, 181);
                  system.endColor = new Color(247, 226, 181);
                  system.minInitialSpeed = 0;
                  system.maxInitialSpeed = 0;
                  system.minRotationSpeed = 0; // -MathHelper.PiOver2 / 5.0f;
                  system.maxRotationSpeed = 0; // MathHelper.PiOver2 / 10.0f;
                  system.minLifetime = 12.0f;
                  system.maxLifetime = 24.0f;
                  system.TimeBetweenEmitting = 3f;
                  system.MinNumParticles = 0;
                  system.MaxNumParticles = 1;
                  system.minScale = 0.75f;
                  system.maxScale = 2.25f;
             //     system.Initialize();
                  list.Add(system);


                  //pollen system  //works best with windspeed 3f, if lower windspeed we need to reduce the rotationspeed of the particles.
                  system = new ParticleSystemType("pollen", 30);
                  system.TextureFilename = "pollen"; // file name / path            
                  system.SystemScale = 0.3f;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.IsAffectedByWind = true;
                  system.MinWindAccelerationFactor = 0.7f;//0.1
                  system.MaxWindAccelerationFactor = 1f;
                  system.StartColor = new Color(255, 255, 255);//(196, 122, 96)
                  system.endColor = new Color(255, 255, 255);
                  system.minInitialSpeed = 1f;
                  system.maxInitialSpeed = 1.9f;
                  system.minRotationSpeed = -MathHelper.PiOver2 / 3f; //minRotationSpeed PiOver2 /0.1f  makes it spin fast
                  system.maxRotationSpeed = MathHelper.PiOver2 / 4.0f;
                  system.minLifetime = 4.0f;
                  system.maxLifetime = 25.0f;
                  system.TimeBetweenEmitting = 5f; //mp apr 2015: this was changed to 102f for some reason!!!
                  system.MinNumParticles = 0;
                  system.MaxNumParticles = 3;
                  //  system.Scale = 1f;
                  system.minScale = .5f;
                  system.maxScale = 1.0f;
             //     system.Initialize();
                  list.Add(system);


                  system = new ParticleSystemType("fireSparks", 30);
                  system.TextureFilename = "pollen"; // file name / path            
                  system.SystemScale = 0.3f;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.IsAffectedByWind = false;
                  system.MinWindAccelerationFactor = 0.1f;//0.1
                  system.MaxWindAccelerationFactor = 0.3f;
                  system.StartColor = new Color(255, 244, 168); //light yellow
                  system.endColor = new Color(255, 83, 15);   //red
                  system.minInitialSpeed = 8f;
                  system.maxInitialSpeed = 12f;
                  system.minRotationSpeed = -MathHelper.PiOver2 / 3f; //minRotationSpeed PiOver2 /0.1f  makes it spin fast
                  system.maxRotationSpeed = MathHelper.PiOver2 / 4.0f;
                  system.minLifetime = 0.1f;
                  system.maxLifetime = 1.5f;
                  system.TimeBetweenEmitting = 0.5f;
                  system.MinNumParticles = 0;
                  system.MaxNumParticles = 3;
                  //  system.Scale = 1f;
                  system.minScale = 0.2f;
                  system.maxScale = 0.2f;
              //    system.Initialize();
                  list.Add(system);

                  system = new ParticleSystemType("poisonPlume", 40); //9); //mp feb 15 currently VERY ugly
                  system.minInitialSpeed = 240;
                  system.maxInitialSpeed = 300;
                  system.minAcceleration = 0;
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "explosion";
                  system.MaxWindAccelerationFactor = 0.5f;
                  system.MinWindAccelerationFactor = 1f;
                  system.StartColor = new Color(0, 255, 0); //green.. duh
                  system.endColor = new Color(255, 255, 0);   //yellow
                  system.IsAffectedByWind = true;
                  system.minRotationSpeed = -MathHelper.PiOver2 / 2.0f;
                  system.maxRotationSpeed = -MathHelper.Pi;
                  system.minScale = 0.3f;
                  system.maxScale = 0.5f;
                  system.MinNumParticles = 5;
                  system.MaxNumParticles = 10;
                  system.minLifetime = 2f;
                  system.maxLifetime = 3f;
                  system.spriteBlendState = BlendState.Additive;
             //     system.Initialize();
                  list.Add(system);

            //bso landmine effect
                  system = new ParticleSystemType("mineExplosion", 2);
                  system.SystemScale = 0.3f;
                  system.minInitialSpeed = 5;
                  system.maxInitialSpeed = 20; 
                  system.minAcceleration = 0;
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "smokeWhite";
                  system.RemoveEmittersAfterLastParticleExpires = true; // one explosion per emitter           
                  system.IsAffectedByWind = false;
                  system.DecelerateParticlesToZeroBeforeTheyDie = true;
                  system.minRotationSpeed = -MathHelper.PiOver4;
                  system.maxRotationSpeed = MathHelper.PiOver4;
                  system.minScale = 0.3f;
                  system.maxScale = 0.75f;
                  system.MinNumParticles = 5;
                  system.MaxNumParticles = 10;
                  system.minLifetime = 0.1f;  // explosions should be relatively short lived
                  system.maxLifetime = 0.4f;
                  system.spriteBlendState = BlendState.Additive;
             //     system.Initialize();
                  list.Add(system);

                  system = new ParticleSystemType("bigBombExplosion1", 2); // not in use - was to big
                  system.SystemScale = 0.5f;
                  system.minInitialSpeed = 9;
                  system.maxInitialSpeed = 25;
                  system.minAcceleration = 0;
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "smokeWhite";
                  system.RemoveEmittersAfterLastParticleExpires = true; // one explosion per emitter           
                  system.IsAffectedByWind = false;
                  system.DecelerateParticlesToZeroBeforeTheyDie = true;
                  system.minRotationSpeed = -MathHelper.PiOver4;
                  system.maxRotationSpeed = MathHelper.PiOver4;
                  system.minScale = 0.5f;
                  system.maxScale = 1f;
                  system.MinNumParticles = 5;
                  system.MaxNumParticles = 10;
                  system.minLifetime = 0.1f;  // explosions should be relatively short lived
                  system.maxLifetime = 0.4f;
                  system.spriteBlendState = BlendState.Additive;
            //      system.Initialize();
                  list.Add(system);

                  system = new ParticleSystemType("bigBombExplosion2", 2); //not in use - was to big
                  system.SystemScale = 0.5f;
                  system.minInitialSpeed = 40;   // high initial speed with lots of variance.  make the values closer
                  system.maxInitialSpeed = 500; // together to have more consistently circular explosions.
                  system.minAcceleration = 0; // we use deceleration instead
                  system.maxAcceleration = 0;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "explosion";
                  system.RemoveEmittersAfterLastParticleExpires = true; // one explosion per emitter           
                  system.IsAffectedByWind = false;
                  system.DecelerateParticlesToZeroBeforeTheyDie = true;
                  system.minRotationSpeed = -MathHelper.PiOver4;
                  system.maxRotationSpeed = MathHelper.PiOver4;
                  system.minScale = 0.5f;
                  system.maxScale = 1.3f;
                  system.MinNumParticles = 5;
                  system.MaxNumParticles = 10;
                  system.minLifetime = 0.5f;  // explosions should be relatively short lived
                  system.maxLifetime = 1f;
                  system.spriteBlendState = BlendState.Additive;
           //       system.Initialize();
                  list.Add(system);

                  system = new ParticleSystemType("gunpowderSmoke", 2);
                  system.TextureFilename = "smoke";
                  system.RemoveEmittersAfterLastParticleExpires = true; // one cloud per emitter
                  system.SystemScale = 0.30f;
                  system.StartMeanMoveDirectionInDegrees = 90;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 0;
                  system.IsAffectedByWind = true;
                  system.MaxWindAccelerationFactor = 1f;
                  system.MinWindAccelerationFactor = 0.5f;
                  system.StartColor = new Color(174, 227, 236);
                  system.endColor = new Color(174, 227, 236);
                  system.minInitialSpeed = 30;
                  system.maxInitialSpeed = 50;
                  system.minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
                  system.maxRotationSpeed = MathHelper.PiOver4 / 2.0f;
                  system.minLifetime = 0.5f;
                  system.maxLifetime = 5f;                      
                  system.MinNumParticles = 10;
                  system.MaxNumParticles = 11;
                  system.minScale = 0.75f;
                  system.maxScale = 1.5f;
            //      system.Initialize();
                  list.Add(system);


                  system = new ParticleSystemType("explosionSmokeCloudLong", 8);
                  system.SystemScale = 0.5f;
                  system.minInitialSpeed = 5;     // less initial speed than the explosion itself
                  system.maxInitialSpeed = 5;
                  system.minAcceleration = 5;  // acceleration is negative, so particles will accelerate away from the
                  system.maxAcceleration = 10; // initial velocity.  this will make them slow down, as if from wind
                  // resistance. we want the smoke to linger a bit and feel wispy, though,
                  // so we don't stop them completely like we do ExplosionParticleSystem
                  // particles.         
                  system.TextureFilename = "smoke";
                  system.RemoveEmittersAfterLastParticleExpires = true; // one explosion per emitter            
                  system.IsAffectedByWind = true;
                  system.minRotationSpeed = -MathHelper.PiOver4;
                  system.maxRotationSpeed = MathHelper.PiOver4;
                  system.minScale = 0.75f;
                  system.maxScale = 1f;
                  system.MinNumParticles = 10;
                  system.MaxNumParticles = 20;
                  system.TimeBetweenEmitting = 20;
                  system.minLifetime = 12f;  // explosion smoke lasts for longer than the explosion itself, but not
                  system.maxLifetime = 12f;    // as long as the plumes do.            
                  system.spriteBlendState = BlendState.AlphaBlend;         
                  list.Add(system);

                  system = new ParticleSystemType("buckShotCloud", 20);
                  system.minInitialSpeed = 600;
                  system.maxInitialSpeed = 800;
                  system.minAcceleration = 0;
                  system.maxAcceleration = -10;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "pollen";   
                  system.RemoveEmittersAfterLastParticleExpires = true; // one smoke cloud per emitter 
                  system.IsAffectedByWind = false;
                  system.minRotationSpeed = 0;
                  system.maxRotationSpeed = 0;
                  system.MinNumParticles = 5;
                  system.MaxNumParticles = 7;
                  system.minScale = 0.18f;
                  system.maxScale = 0.18f;
                  system.StartColor = new Color(0, 0, 0);
                  system.endColor = new Color(0, 0, 0, 0.5f); 
                  system.minLifetime = 0.15f;
                  system.maxLifetime = 0.25f;
               
                //  system.TimeBetweenEmitting = 10f;              
                  list.Add(system);

                  system = new ParticleSystemType("goldBuckShotCloud", 20);
                  system.minInitialSpeed = 600;
                  system.maxInitialSpeed = 800;
                  system.minAcceleration = 0;
                  system.maxAcceleration = -10;
                  system.StartMaxMoveDirectionDifferenceInDegrees = 10;
                  system.TextureFilename = "pollen";  
                  system.RemoveEmittersAfterLastParticleExpires = true; // one smoke cloud per emitter 
                  system.IsAffectedByWind = false;
                  system.minRotationSpeed = 0;
                  system.maxRotationSpeed = 0;
                  system.MinNumParticles = 5;
                  system.MaxNumParticles = 7;
                  system.minScale = 0.18f;
                  system.maxScale = 0.18f;
                  system.StartColor = new Color(255, 215, 0);
                  system.endColor = new Color(255, 215, 0, 0.5f);
                  system.minLifetime = 0.15f;
                  system.maxLifetime = 0.25f;                
              //    system.TimeBetweenEmitting = 10f;          
                  list.Add(system);

                  system = new ParticleSystemType("gunSmoke", 8);
                  system.SystemScale = 0.05f;
                  system.minInitialSpeed = 490;
                  system.maxInitialSpeed = 590;
                  system.minAcceleration = -15;
                  system.maxAcceleration = -20;
                  system.TextureFilename = "smoke";
                  system.RemoveEmittersAfterLastParticleExpires = true;// one smoke cloud per emitter 
                  system.IsAffectedByWind = true;
                  system.MinWindAccelerationFactor = 3;
                  system.MaxWindAccelerationFactor = 3;
                  system.minRotationSpeed = -MathHelper.PiOver4;
                  system.maxRotationSpeed = MathHelper.PiOver4;
                  system.StartColor = new Color(200, 200, 200);
                  system.endColor = new Color(200, 200, 200, 0.5f); 
                  system.minScale = 1f;
                  system.maxScale = 6f;
                  system.MinNumParticles = 8;
                  system.MaxNumParticles = 14;
                  //system.TimeBetweenEmitting = 50;
                  system.minLifetime = 3f;
                  system.maxLifetime = 6f;              
                  system.spriteBlendState = BlendState.AlphaBlend;        
                  list.Add(system);

                  return list;
        }

       
    }
}

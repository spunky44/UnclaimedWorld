using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Xclna.Xna.Animation;
using UWGame.Client.Particles;
using UWGame.SimSide.Entities;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using UWGame.ClientSide.Renderables;
using UWGame.Client.Audio;

namespace UWGame.SimSide.Entities
{
    public interface IStateInfo
    {
        BitMask64 Conditions
        {
            get;
        }

        /// <summary>
        /// the info will never be chosen when the entity has one of these states
        /// </summary>
        BitMask64 Forbiddens
        {
            get;
        }
    }

    /// <summary>
    /// Matches status flags to sprites, particles, sounds etc. Same pattern as for animations.
    /// </summary>
    [DebuggerDisplay("Conditions:{Conditions} Forbiddens:{Forbiddens.StateNames}")]
    public class ClientStateInfo : IStateInfo
    {
       
        /// <summary>
        /// TODO: XML serialize these as an array/sequence of bits (binary?)
        /// </summary>     
        public BitMask64 Conditions { get; set; }

        /// <summary>
        /// the info will never be chosen when the entity has one of these states
        /// </summary>
        public BitMask64 Forbiddens { get; set; }

        public RenderAsBillboardType[] RenderAsBillboardType;
        public RenderAsGroundSpriteType RenderAsGroundSpriteType;

     
        
        /// <summary>
        /// particles associated with the state
        /// </summary>
        public ParticleEmitterEffect[] ParticleEmitters;

        /// <summary>
        /// lights to use
        /// </summary>
        public LightingType[] LightingTypes;

        /// <summary>
        /// 
        /// </summary>
        public NormalDistribution DelayBetweenSounds;

       
        public string[] Sounds;


        [XmlIgnore]
        public SoundData[] SoundDatas
        {
            get;
            private set;
        }


       

        public ClientStateInfo()
        {
            // default???
         //   AnimationSet = new RandomAnimationSet() { BaseAnimations = new string[] { "idle" } }; 
        }
        
      
      

        public bool Test(StateModifier state)
        {
            if (Conditions != null && Conditions.Test(state))
            {
                return true;
            }

            return false;
        }

        internal void Initialize()
        {
            if (ParticleEmitters != null)
            {
                foreach (var item in ParticleEmitters)
                {
                    item.Initialize();
                }
            }

           
           
            if (Sounds != null)
            {
                SoundDatas = new SoundData[Sounds.Length];

                for (int i = 0; i < Sounds.Length; i++)
                {
                    SoundDatas[i] = GameData.Instance.AllSoundData[Sounds[i]];
                }

            }
        
        }


        private bool RequiresGhostedImage(EntityType entityType)
        {
            if (entityType.GetUsesMemory() || entityType.StructureType != null)
            {
                return true;
            }

            return false;
        }


        internal void PostLoadContentValidate(EntityType parent, ref List<string> listOfErrors)
        {
            if (RenderAsBillboardType != null)
            {
                foreach (var item in RenderAsBillboardType)
                {
                    Rectangle? rect;
                    if (!string.IsNullOrEmpty(item.AssetName))
                    {
                        if (!GameData.Instance.BillboardSpriteSheet.TryGetSourceRectangle(item.AssetName, out rect))
                        {
                            EntityType.CreateValidationError(ref listOfErrors, "Billboard asset " + item.AssetName + " not found in BillboardSpriteSheet.");

                        }
                        if (RequiresGhostedImage(parent)
                            && !The.Client.Renderer.GhostedStructuresSpriteSheet.TryGetSourceRectangle(item.AssetName, out rect))
                        {
                            EntityType.CreateValidationError(ref listOfErrors, "Billboard asset " + item.AssetName + " not found in GhostedStructuresSpriteSheet.");

                        }
                    }
                }
            }

            if (RenderAsGroundSpriteType != null)
            {

                Rectangle? rect;
                if (!The.Client.FlatSpriteSheet.TryGetSourceRectangle(RenderAsGroundSpriteType.AssetName, out rect))
                {
                    EntityType.CreateValidationError(ref listOfErrors, "Flat sprite asset " + RenderAsGroundSpriteType.AssetName + " not found.");

                }
            }

        }
    }

    /// <summary>
    /// New footprint.
    /// This cannot be in RenderableType, since it is Sim stuff
    /// 
    /// </summary>
    public class SimStateInfo : IStateInfo
    {

        public BitMask64 Conditions { get; set; }

        public BitMask64 Forbiddens { get; set; }

        // could also contain other sim stuff

        /// <summary>
        /// the geo layout to use (Sim side!)
        /// </summary>
        public GeometryLayoutType GeometryLayoutType;
    }

}

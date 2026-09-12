using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.ClientSide.Particles;
using Microsoft.Xna.Framework;

namespace UWGame.Client.Particles
{
    /// <summary>
    /// for use in EntityTypes, animation infos, actions etc.
    /// </summary>
    public class ParticleEmitterEffect
    {
        public string ParticleSystemKey;

        public float Intensity = 1f;

        public bool EmitParticlesInParentDirection = false;
        
        /// <summary>
        /// can either spawn at a location or attach to an entity until it gets destroyed...
        /// </summary>
        public bool AttachToEntity = true;

        /// <summary>
        /// an offset to the parent location that we can use when attached. Should be small, since the emitter location will determine visibility of the emitted particles
        /// </summary>
        public Vector2 Offset;

        /// <summary>
        /// if null, the emitter is permanent (or, if attached, until the renderable gets destroyed)
        /// </summary>
        public double? DurationInSeconds;

        public void Initialize()
        {
            // client does not exist at this point...
           // ParticleSystem = The.Client.ParticleManager.ParticleSystems[ParticleSystemKey];
        }

    }
}

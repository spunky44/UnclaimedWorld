using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    public enum DetectableID : ulong
    {
        First = 0L,
        Invalid = uint.MaxValue,
        Max = Invalid
    }

    public interface IDetectable : ILookUp<IDetectable, DetectableID>
    {
       // float GetAvoidDetectionFactor();

        /// <summary>
        /// only one of the below are filled
        /// </summary>
        EntityType EntityType { get; }
        ResourceType ResourceType { get;  }

        Vector3 Location { get; }

        bool IsIntelligent { get; }

     //   bool IsEatable(EntityType inquiringEntityType);

        bool UsesMemory(SharedKnowledge sharedKnowledge);
        
      //  void SeeByAllegiance(Allegiances.Allegiance allegiance);

        bool RequiresRollToDetect();

        string ToLink(bool useUpperCase = false);
    }
}

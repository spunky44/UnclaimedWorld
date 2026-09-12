using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.ClientSide.Renderables
{
    /// <summary>
    /// Be mindful when adding KindOfTypes. Most ideas you can come up with are probably just aggregations of existing kindofs
    /// try your best to build your kindof with carefully chosen IS and IS-NOT cases of other kindofs
    /// many examples are embedded in the enum declaration, as comments, please read
    /// These constants represent the bit order in a bitmask, so do not assign values 0-63 to anything not to be used, like 'invalid' or 'none'
    /// </summary>
    public enum RenderKindOfType
    {
        //Don't confuse the two. EntityType Kindofs have to do with running the simulation
        //and are relevant even when running headless (without a client -- no screen or sound)
        Selectable,         //can be clicked to select it
        CastsShadow,        // draws a shadow of some kind
        DontHideIfFogged,   //stays rendered, even in the fog of war
        ClickThru,          // will allow click to go to thing behind
        ReactsToSelect,     // does something in UI or client when selected
        ChangesWithTimeOfDay,
        ChangesWithSeason,
        MadeOfDirt,         // for impacts and such, what kinds of particles or sounds to emit
        MadeOfStone,
        MadeOfMetal,
        MadeOfWood,
        MadeOfAnimal,
        MadeOfPlant
    }
}

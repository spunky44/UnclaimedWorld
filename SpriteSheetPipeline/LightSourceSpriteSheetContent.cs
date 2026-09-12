#region File Description
//-----------------------------------------------------------------------------
// SpriteSheetContent.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content;
using SpriteSheetRuntime;
#endregion

namespace SpriteSheetPipeline
{
    /// <summary>
    /// Build-time type used to hold the output data from the SpriteSheetProcessor.
    /// This is saved into XNB format by the SpriteSheetWriter helper class, then
    /// at runtime, the SpriteSheetReader loads the data into a SpriteSheet object.
    /// </summary>
    [ContentSerializerRuntimeType("SpriteSheetRuntime.LightSourceSpriteSheet, SpriteSheetRuntime")]
    public class LightSourceSpriteSheetContent: SpriteSheetContent
    {
       // public Dictionary<string, LightSourceTypeContent> LightSourceTypes = new Dictionary<string, LightSourceTypeContent>();
        public Dictionary<string, LightSourceType> AllLightSourceData = new Dictionary<string, LightSourceType>();
    }
}

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
    [ContentSerializerRuntimeType("SpriteSheetRuntime.ExtendedSpriteSheet, SpriteSheetRuntime")]
    public class ExtendedSpriteSheetContent: SpriteSheetContent
    {
        public Texture2DContent NormalTexture = new Texture2DContent();

     //   public Dictionary<string, TreeTypeData> TreeTypeData = new Dictionary<string, TreeTypeData>();

      //  public Dictionary<string, BuildingTypeData> BuildingTypeData = new Dictionary<string, BuildingTypeData>();
       
    }
}

#region File Description
//-----------------------------------------------------------------------------
// SpriteSheetProcessor.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;
#endregion

namespace SpriteSheetPipeline
{
    
    [ContentProcessor]
    public class LightSourceSpriteSheetProcessor : ContentProcessor<string[], LightSourceSpriteSheetContent>
    {       

        bool doDebugLogging = false;

        LightSourceSpriteSheetContent spriteSheet;

        /// <summary>
        /// Converts an array of sprite filenames into a sprite sheet object.
        /// </summary>
        public override LightSourceSpriteSheetContent Process(string[] input,
                                                   ContentProcessorContext context)
        {
            context.Logger.LogImportantMessage("Start processing light source sprites.");

            spriteSheet = new LightSourceSpriteSheetContent();
             
            List<BitmapContent> sourceSprites = new List<BitmapContent>();
         //   Dictionary<string, LightSourceTypeContent> lightSourceTypes = new Dictionary<string, LightSourceTypeContent>();

            bool hasLogged = false;

            // Loop over each input sprite filename.
            foreach (string inputFilename in input)
            {
                if (inputFilename.EndsWith("\\"))
                {
                    context.Logger.LogImportantMessage("Processing light source sprites in folder {0}", inputFilename);
                    string folderName = inputFilename.Substring(0, inputFilename.Length - 1);

                    string[] filesInFolder = System.IO.Directory.GetFiles(folderName);
                    foreach (string filename in filesInFolder)
                    {                        
                        string spriteName = Path.GetFileNameWithoutExtension(filename);

                        context.Logger.LogImportantMessage("Look at file {0}, suffix: {1}, extension: {2}, spritename: {3}", 
                            filename, spriteName.Substring(spriteName.Length - 2, 1), Path.GetExtension(filename), spriteName);

                        if (Path.GetExtension(filename) == ".tga") // && spriteName.Substring(spriteName.Length - 2, 1) != "_")
                        { 
                            // Store the name of this sprite.                            
                            spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);

                            AddSpriteAndExtractInfo(context, sourceSprites, filename, spriteName);


                        }
                    }
                }
                else
                {
                    // load single sprite
                    // Store the name of this sprite.
                    string spriteName = Path.GetFileNameWithoutExtension(inputFilename);
                    spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);

                    AddSpriteAndExtractInfo(context, sourceSprites, inputFilename, spriteName);

                }    
            }

            // Pack all the sprites into a single large texture.
            
            PixelBitmapContent<NormalizedByte4> packedSpritesNormalMaps = null;
            BitmapContent packedSprites = null;

            SpritePacker.PackSprites(sourceSprites, spriteSheet.SpriteNames, null, 
                                     spriteSheet.SpriteRectangles, context,
                                     out packedSprites, out packedSpritesNormalMaps);//, false); 

            spriteSheet.Texture.Mipmaps.Add(packedSprites);
            context.Logger.LogImportantMessage("Added diffuse sprite sheet texture.");
           

            return spriteSheet;
        }


        private void AddSpriteAndExtractInfo(ContentProcessorContext context, 
                                             List<BitmapContent> sourceSprites, 
                                             string inputFilename, string spriteName)
        {
            ExternalReference<TextureContent> textureReference =
                            new ExternalReference<TextureContent>(inputFilename);
             
            context.Logger.LogImportantMessage("Adding diffuse sprite {0}", inputFilename);
            TextureContent texture = context.BuildAndLoadAsset<TextureContent, TextureContent>(textureReference, null);
            context.Logger.LogImportantMessage("NOW Adding diffuse sprite {0}", inputFilename);
            
            // add sprite texture to list - it is later made into the spritesheet texture:
            sourceSprites.Add(texture.Faces[0][0]);

          /*  PixelBitmapContent<Vector4> bitmap;
            bitmap = (PixelBitmapContent<Vector4>)texture.Faces[0][0];
            */
            PixelBitmapContent<Color> bitmap;
            bitmap = (PixelBitmapContent<Color>)texture.Faces[0][0];
            context.Logger.LogImportantMessage("Adding light source information");
            
            LightSourceType lightSource = new LightSourceType(); 

            string[] spriteNameSplit = spriteName.Split('_');
            
            string lightSourceID;
            if (spriteNameSplit[0] == "common")
            {
                lightSourceID = spriteNameSplit[1];
            }
            else
            {
                lightSourceID = spriteNameSplit[2];
                lightSource.IsIndoor = spriteNameSplit[1] == "i";
            }

            lightSource.SpriteName = spriteName;
            // extract the offset pixel:
            Point offset = GetOffset(bitmap, false, context);
            lightSource.Offset = offset;
          //  context.Logger.LogImportantMessage("light source offset: " + offset.ToString());
            offset.X = bitmap.Width - offset.X;
            lightSource.OffsetFlipped = offset;            
          //  context.Logger.LogImportantMessage("light source offset flipped: " + offset.ToString());
            
            //lightSource.SpriteRectangle = spriteSheet.SpriteRectangles
            spriteSheet.AllLightSourceData.Add(spriteNameSplit[0] + "_" + lightSourceID, lightSource); //new LightSourceTypeContent(){ Offset = GetOffset(bitmap, false, context)});

        }

        public static Color Sample4EdgesOfPixel(PixelBitmapContent<Color> bitmap, int x, int y)
        {
           /* Color newValue = Color.Black;
            newValue.A = 0;
            */
            Vector4 newValue = Vector4.Zero;
            int noOfSamples = 0;

            if (x > 0) 
            {
                     
                newValue = newValue + bitmap.GetPixel(x - 1, y).ToVector4();      
                noOfSamples++;
            }
            if (x < bitmap.Width - 1) 
            {
                newValue = newValue + bitmap.GetPixel(x + 1, y).ToVector4();
                noOfSamples++;
            }
            if (y > 0)
            {
                newValue = newValue + bitmap.GetPixel(x, y - 1).ToVector4();
                noOfSamples++;
            }
            if (y < bitmap.Height - 1)
            {
                newValue = newValue + bitmap.GetPixel(x, y + 1).ToVector4();
                noOfSamples++;
            }

            newValue = newValue / noOfSamples;
            return new Color(newValue);
        }

        public static Vector4 Sample4EdgesOfPixel(PixelBitmapContent<Vector4> bitmap, int x, int y)
        {
            Vector4 newValue = Vector4.Zero;
            int noOfSamples = 0;

            if (x > 0)
            {
                newValue = newValue + bitmap.GetPixel(x - 1, y);
                noOfSamples++;
            }
            if (x < bitmap.Width - 1)
            {
                newValue = newValue + bitmap.GetPixel(x + 1, y);
                noOfSamples++;
            }
            if (y > 0)
            {
                newValue = newValue + bitmap.GetPixel(x, y - 1);
                noOfSamples++;
            }
            if (y < bitmap.Height - 1)
            {
                newValue = newValue + bitmap.GetPixel(x, y + 1);
                noOfSamples++;
            }

            return newValue / noOfSamples;
        }

        static Point GetOffset(PixelBitmapContent<Color> bitmap, bool doDebugLogging, ContentProcessorContext context)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color value = bitmap.GetPixel(x, y);

                    if (value.A < 10) // alpha channel has marker!
                    {
                        //value.A = 255;
                       // bitmap.SetPixel(x, y, value); 
                        bitmap.SetPixel(x, y, Sample4EdgesOfPixel(bitmap, x, y));
                        return new Point(x, y);
                    }
                    
                }
            }

            return Point.Zero;
        }

        
    }
}
